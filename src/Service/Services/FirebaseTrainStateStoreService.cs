using Firebase.Database;
using Firebase.Database.Query;
using MetroShip.Service.ApiModels.Train;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Enums;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public async Task SetDirectionAsync(string trainId, DirectionEnum direction)
            => await _firebase.Child("train_state").Child(trainId).Child("Direction").PutAsync(direction.ToString());

        public async Task<DirectionEnum?> GetDirectionAsync(string trainId)
        {
            var val = await _firebase.Child("train_state").Child(trainId).Child("Direction").OnceSingleAsync<string>();
            return Enum.TryParse<DirectionEnum>(val, true, out var d) ? d : (DirectionEnum?)null;
        }

        public async Task SetSegmentIndexAsync(string trainId, int index)
            => await _firebase.Child("train_state").Child(trainId).Child("SegmentIndex").PutAsync(index);

        public async Task<int?> GetSegmentIndexAsync(string trainId)
        {
            try { return await _firebase.Child("train_state").Child(trainId).Child("SegmentIndex").OnceSingleAsync<int>(); }
            catch { return null; }
        }

        public async Task SetStartTimeAsync(string trainId, DateTimeOffset startTime)
            => await _firebase.Child("train_state").Child(trainId).Child("StartTime").PutAsync(startTime.ToUnixTimeSeconds());

        public async Task<DateTimeOffset?> GetStartTimeAsync(string trainId)
        {
            try
            {
                var unix = await _firebase.Child("train_state").Child(trainId).Child("StartTime").OnceSingleAsync<long>();
                return DateTimeOffset.FromUnixTimeSeconds(unix);
            }
            catch { return null; }
        }

        public async Task SetPositionResultAsync(string trainId, TrainPositionResult result)
            => await _firebase.Child("train_position").Child(trainId).PutAsync(result);

        public async Task<TrainPositionResult?> GetPositionResultAsync(string trainId)
        {
            try { return await _firebase.Child("train_position").Child(trainId).OnceSingleAsync<TrainPositionResult>(); }
            catch { return null; }
        }
    }
}
