name: .NET CI

on:
  push:
branches: [main, develop]
pull_request:
branches: [main, develop]

jobs:
build - and - test:
    runs - on: ubuntu - latest

    steps:
-name: Checkout code
      uses: actions / checkout@v3

    - name: Setup.NET
      uses: actions / setup - dotnet@v4
      with:
        dotnet - version: '8.0.x'

    - name: Restore dependencies
      run: dotnet restore

    - name: Build solution
      run: dotnet build --no-restore --configuration Release

    - name: Run unit tests
      run: dotnet test MSPR-bloc-4-products.tests/MSPR-bloc-4-products.tests.csproj --no-build --configuration Release --logger trx

    - name: Run integration tests
      run: dotnet test MSPR-bloc-4-products.test-integration/MSPR-bloc-4-products.test-integration.csproj --no-build --configuration Release --logger trx
