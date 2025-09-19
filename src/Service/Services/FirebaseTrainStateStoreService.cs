using Firebase.Database;
using Firebase.Database.Query;
using MetroShip.Service.ApiModels.Train;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Enums;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace MetroShip.Service.Services
{
    public class FirebaseTrainStateStoreService : ITrainStateStoreService
    {
        private readonly FirebaseClient _firebase;

        public FirebaseTrainStateStoreService(IConfiguration config)
        {
            _firebase = new FirebaseClient(
                config["Firebase:Url"],
                new FirebaseOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(config["Firebase:Secret"])
                });
        }

        #region Set methods
        public async Task SetDirectionAsync(string trainId, DirectionEnum direction)
        {
            // Convert enum to string và bọc JSON
            var jsonValue = $"\"{direction}\""; // => "Forward"
            await _firebase.Child("train_state").Child(trainId).Child("Direction").PutAsync(jsonValue);
        }

        public async Task SetSegmentIndexAsync(string trainId, int index)
            => await _firebase.Child("train_state").Child(trainId).Child("SegmentIndex").PutAsync(index);

        public async Task SetStartTimeAsync(string trainId, DateTimeOffset startTime)
            => await _firebase.Child("train_state").Child(trainId).Child("StartTime").PutAsync(startTime.ToUnixTimeSeconds());

        public async Task SetPositionResultAsync(string trainId, TrainPositionResult result)
            => await _firebase.Child("train_position").Child(trainId).PutAsync(result);
        #endregion

        #region Get methods
        public async Task<DirectionEnum?> GetDirectionAsync(string trainId)
        {
            var val = await _firebase.Child("train_state").Child(trainId).Child("Direction").OnceSingleAsync<string>();
            return Enum.TryParse<DirectionEnum>(val, true, out var d) ? d : (DirectionEnum?)null;
        }

        public async Task<int?> GetSegmentIndexAsync(string trainId)
        {
            try { return await _firebase.Child("train_state").Child(trainId).Child("SegmentIndex").OnceSingleAsync<int>(); }
            catch { return null; }
        }

        public async Task<DateTimeOffset?> GetStartTimeAsync(string trainId)
        {
            try
            {
                var unix = await _firebase.Child("train_state").Child(trainId).Child("StartTime").OnceSingleAsync<long>();
                return DateTimeOffset.FromUnixTimeSeconds(unix);
            }
            catch { return null; }
        }

        public async Task<TrainPositionResult?> GetPositionResultAsync(string trainId)
        {
            try { return await _firebase.Child("train_position").Child(trainId).OnceSingleAsync<TrainPositionResult>(); }
            catch { return null; }
        }

        public async Task<Dictionary<string, (DirectionEnum? direction, int? segmentIndex)>> GetDirectionsAndSegmentIndicesAsync(
            List<string> trainIds)
        {
            var result = new Dictionary<string, (DirectionEnum? direction, int? segmentIndex)>();

            // Get all train_state data in one call
            var allTrainStates = await _firebase.Child("train_state")
                .OnceSingleAsync<Dictionary<string, Dictionary<string, object>>>();

            if (allTrainStates != null)
            {
                foreach (var trainId in trainIds)
                {
                    DirectionEnum? direction = null;
                    int? segmentIndex = null;

                    if (allTrainStates.TryGetValue(trainId, out var trainData))
                    {
                        // Parse Direction
                        if (trainData.TryGetValue("Direction", out var directionObj) && directionObj != null)
                        {
                            if (Enum.TryParse<DirectionEnum>(directionObj.ToString(), true, out var parsedDirection))
                            {
                                direction = parsedDirection;
                            }
                        }

                        // Parse SegmentIndex
                        if (trainData.TryGetValue("SegmentIndex", out var segmentObj) && segmentObj != null)
                        {
                            if (int.TryParse(segmentObj.ToString(), out var parsedSegment))
                            {
                                segmentIndex = parsedSegment;
                            }
                        }
                    }

                    result[trainId] = (direction, segmentIndex);
                }
            }

            return result;
        }
        #endregion

        #region Remove methods
        public async Task RemoveDirectionAsync(string trainId)
            => await _firebase.Child("train_state").Child(trainId).Child("Direction").DeleteAsync();

        public async Task RemoveSegmentIndexAsync(string trainId)
            => await _firebase.Child("train_state").Child(trainId).Child("SegmentIndex").DeleteAsync();

        public async Task RemoveStartTimeAsync(string trainId)
            => await _firebase.Child("train_state").Child(trainId).Child("StartTime").DeleteAsync();

        public async Task RemovePositionResultAsync(string trainId)
            => await _firebase.Child("train_position").Child(trainId).DeleteAsync();

        public async Task RemoveAllTrainStateAsync(string trainId)
        {
            await RemoveDirectionAsync(trainId);
            await RemoveSegmentIndexAsync(trainId);
            await RemoveStartTimeAsync(trainId);
            await RemovePositionResultAsync(trainId);
        }
        #endregion

        #region Has methods
        public async Task<bool> HasDirectionAsync(string trainId)
            => await GetDirectionAsync(trainId) != null;

        public async Task<bool> HasSegmentIndexAsync(string trainId)
            => await GetSegmentIndexAsync(trainId) != null;

        public async Task<bool> HasStartTimeAsync(string trainId)
            => await GetStartTimeAsync(trainId) != null;

        public async Task<bool> HasPositionResultAsync(string trainId)
            => await GetPositionResultAsync(trainId) != null;
        #endregion

        #region Shipment Tracking
        public async Task SetShipmentTrackingAsync(string trackingCode, string trainId, TrainPositionResult position)
        {
            var data = new
            {
                TrainId = trainId,
                LastPosition = position,
                LastUpdate = DateTimeOffset.UtcNow
            };

            await _firebase
                .Child("shipment_tracking")
                .Child(trackingCode)
                .PutAsync(data);
        }

        public async Task<dynamic?> GetShipmentTrackingAsync(string trackingCode)
        {
            try
            {
                return await _firebase
                    .Child("shipment_tracking")
                    .Child(trackingCode)
                    .OnceSingleAsync<dynamic>();
            }
            catch
            {
                return null;
            }
        }

        public async Task RemoveShipmentTrackingAsync(string trackingCode)
            => await _firebase.Child("shipment_tracking").Child(trackingCode).DeleteAsync();

        public async Task<bool> HasShipmentTrackingAsync(string trackingCode)
            => await GetShipmentTrackingAsync(trackingCode) != null;
        #endregion

        #region List methods
        public async Task<List<string>> GetAllActiveTrainIdsAsync()
        {
            var trains = new List<string>();

            var allStates = await _firebase
                .Child("train_state")
                .OnceAsync<object>();

            foreach (var state in allStates)
            {
                var trainId = state.Key;

                if (await HasStartTimeAsync(trainId) && await HasSegmentIndexAsync(trainId))
                    trains.Add(trainId);
            }

            return trains;
        }
        #endregion
    }
}
