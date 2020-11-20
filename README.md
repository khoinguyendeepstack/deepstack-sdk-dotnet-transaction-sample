# Globally Paid .NET SDK Samples (Examples of use)

The official [Globally Paid .NET library][gp-dotnet] samples

## Usage

Clone this repository

### Sample Console App

Sample console app that showcases all Globally Paid SDK service calls.

##### Configuration

All SDK calls in the Sample Console App can be configured using the static `GloballyPaidConfiguration` object:

```c#
GloballyPaidConfiguration.PublishableApiKey = "Your Publishable API Key";
GloballyPaidConfiguration.SharedSecret = "Your Shared Secret";
GloballyPaidConfiguration.AppId = "Your APP ID";
GloballyPaidConfiguration.UseSandbox = false; //true if you need to test through Globally Paid sandbox
GloballyPaidConfiguration.RequestTimeoutSeconds = 90;
```
Or using the `GloballyPaidConfiguration` *Setup* method:

```c#
GloballyPaidConfiguration.Setup("Your Publishable API Key", "Your Shared Secret", "Your APP ID", useSandbox: false, requestTimeoutSeconds: 90);
```

Or using the `<appSettings>` section in configuration file (App.config):

```xml
<appSettings>
    <add key="GloballyPaidPublishableApiKey" value="Your Publishable API Key"></add>
    <add key="GloballyPaidSharedSecret" value="Your Shared Secret"></add>
    <add key="GloballyPaidAppId" value="Your APP ID"></add>
    <add key="GloballyPaidUseSandbox" value="false"></add> <!--true if you need to test through Globally Paid sandbox-->
    <add key="GloballyPaidRequestTimeoutSeconds" value="90"></add>
</appSettings>
```

##### Run
Run the Sample Console App and see all examples in `Program.cs`

### Sample App

Sample MVC app that showcases charge sale transaction done through a UI credit card form using the [Globally Paid JS SDK][gp-js]

##### Configuration

All SDK calls can be configured within `ConfigureServices` method in `Startup` class using the `AddGloballyPaid` extension.
Additionally, this extension call will register all Globally Paid services:

```c#
services.AddGloballyPaid("Your Publishable API Key", "Your Shared Secret", "Your APP ID", useSandbox: false, requestTimeoutSeconds: 90);
```

To register the Globally Paid services only, `AddGloballyPaidServices` extension can be used:

```c#
services.AddGloballyPaidServices();
```

Or using the `appSettings.{Environment}.json` file:

```json
  "GloballyPaid": {
    "PublishableApiKey": "Your Publishable API Key",
    "SharedSecret": "Your Shared Secret",
    "AppId": "Your APP ID",
    "UseSandbox": true,
    "RequestTimeoutSeconds": 90
  }
```

##### Run
Run the Sample MVC App, populate with test data and click `Submit`

---
For any feedback or bug/enhancement report, please [open an issue][issues] or [submit a
pull request][pulls].

[gp-dotnet]: https://github.com/globallypaid/globallypaid-sdk-dotnet
[gp-js]: https://github.com/globallypaid/js-sdk-v2-sample
[issues]: https://github.com/globallypaid/globallypaid-sdk-dotnet-samples/issues/new
[pulls]: https://github.com/globallypaid/globallypaid-sdk-dotnet-samples/pulls