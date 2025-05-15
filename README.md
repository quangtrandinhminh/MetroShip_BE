# Prọect Name

## Getting Started

To get started with this project, follow these steps:

### Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [Git](https://git-scm.com/)

### Installation

1. Clone the repository:
    ```bash
    git clone https://github.com/quangtrandinhminh/MetroShip_BE.git
    ```

2. Install dependencies:
    ```bash
    dotnet restore
    ```

### Setting Up the Database

Update the `appsettings.json` file with your database connection string:
    ```json
    {
      "ConnectionStrings": {
        "DefaultConnection": "YourConnectionStringHere"
      }
    }
    ```

    **Please do not commit any changes in `appsettings.json`**

### Running the Application

Run the application:
    ```bash
    dotnet run
    ```

## Collaborate with Your Team

To collaborate within the team without forking, follow these steps:

### Branching Strategy

We use a Git branching strategy with three main branches:

- `main`: Contains the stable version of the code. Direct commits to this branch are restricted.
- `dev`: Contains the latest development changes. This is the main branch for ongoing development.
- `test`: Contains code that is under testing before being merged into `dev`.

### Working on a Feature

1. Create a new branch from `dev` for your feature:
    ```bash
    git checkout dev
    git pull origin dev
    git checkout -b feature/your-feature-name
    ```

2. Make your changes and commit them:
    ```bash
    git add .
    git commit -m 'Add some feature'
    ```

3. Push your branch to the repository:
    ```bash
    git push origin feature/your-feature-name
    ```

4. Create a pull request (PR) from your feature branch to `test` for testing:
    - Ensure all tests pass before requesting a merge.
    - Team members review and approve the PR.
    - Once approved, the PR is merged into `test`.

5. After thorough testing, create a pull request from `test` to `dev`.

6. For releases, create a pull request from `dev` to `main`.

## Contributing

We welcome contributions! To contribute:

1. Fork the repository.
2. Create a new branch: `git checkout -b feature/your-feature-name`
3. Make your changes and commit them: `git commit -m 'Add some feature'`
4. Push to the branch: `git push origin feature/your-feature-name`
5. Open a pull request.

## Authors and Acknowledgment

Thanks to all the contributors who have helped develop this project.
