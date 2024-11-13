### Purpose of the EmStockService

EmStockService is intended to get and process the information about the instruments prices from the external data streams.
Read stocks prices from different data streams, process the prices for each stock based on a set of rules to
decide on the price to use for each stock and publish the selected value for each stock to a number of
subscribers.

## Technologies
- .NET 8.0 Hosted Service
- WebSockets (this part is not intended to be implemented within the test task)
- XUnit

Possible TODO:
- Implement the sockets for the broadcasting the symbol details across the subscribers
- Add the DataAccess project where it would be implemented the repository pattern for the data access and it will be obtained the Em Stock service configuration (as an example using the EF)
- Inject the logging and exception handling to the service
- Extend the error handling logic
- Logging switch to injecting the ILogger to the service

## Getting Started
- Make EmStaockService as a Startup project
- Run (Or just navigate to the EmStockService folder and run "dotnet run" in the console)
- All the events updates could be tracked in the Application console output
- Run the tests in the EmStockService.Tests project, the implmented functionality could be checked with these tests

## Quick notes

This is the test solution for the EmStockService. 
The service is intended to get and process the information about the instruments prices from the external data streams.
The EmStockService is implemented as a Hosted Service that intended to have the WebSocket server for broadcasting the symbol details across the subscribers.
As per the solution structure - I would rather to use Clean Architecture, but for the sake of the test task, I've implemented the solution with the structure 
within one project for the simplification purpose.
Also, the current solution is need to be improved with the configuration of the pre-defined available symbols that prices could be retrieved from the data provider, ie. subscribing on the specific pre-defiened list of the instruments.

As an imrovement in the current implementation the values for the Settings initialization in the GetSettings() should be moved to the appsettings configuration,
but do not see the sense of implementing it as this class is also replacable by the Repository implementation and getting the data from the DB in the DataAccess.
As per the implementaion of the subscribe and unsubscribe methods for the dara sources - it could be implemented with the help of the WebSockets (as an example - SignalR) or binding 
as a subscriber to the RabbitMQ Queue depending on the integration implementation.

PS: The service is not intended to be used in the production environment, it is just a test solution.
