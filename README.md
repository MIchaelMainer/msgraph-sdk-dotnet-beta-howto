# msgraph-sdk-dotnet-beta

This repo contains an example class library solution and workflow for generating beta Microsoft Graph models. It shows how to use the beta models in a solution that already uses the Microsoft Graph client library. This workflow will show you how to create a beta metadata file for the beta Trending API, generate C# model code files for the beta Trending API, and then show how you can integrate the beta Trending API objects in an existing .Net Microsoft Graph solution.

## About the Trending API

For our example, we are using the Trending API. The Trending API provides insights about which documents in OneDrive and SharePoint are trending around a user. While I don't know the secret sauce for what defines the trend around documents, I'd wager it has, in part, something to do with how often documents are accessed. Let's take a look at the Trending API by using the [Graph Explorer](https://developer.microsoft.com/en-us/graph/graph-explorer).  

<!-- TODO: Add image for Graph Explorer and the Trending API; Add 2 slides to deck (include link to this repo); Update Windows as Edge is giving me issues. -->

## Why use the beta Microsoft Graph APIs

We've had some requests for a beta client library for Microsoft Graph. We don't have plans to produce a beta client library. At the same time, we understand that you may want to include beta features. If you haven't considered beta features in your application, here are some reasons why you might want to include beta features in your existing Microsoft Graph client:
* Curiosity - you just want to see what the beta feature does and how it works.
* Create experimental features in your application.
* Get a head start on implementing 'soon to be released' features.

<!-- TODO: Add this information to a slide. -->

## Step 1. Build the SDK generation tool

We use the https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator to generate our code files. It has a submodule called VIPR that loads the metadata into memory and parses out the relationships between metadata objects into an object model that is understood by the code generator and its templates.

1. Recursively clone the repo: 

        git clone --recursive https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator.git

2. Build the solution (F6). Make sure that the Nuget packages are restored.
3. Check that we are targeting C#. Open GraphODataTemplateWriter/.config/TemplateWriterSettings.json and check that the **TargetLanguage** is set to `CSharp`. Note that this tool is used to target other platforms in addition to .Net so the last checkin might have changed the target language.

At this point, our code generator is setup and ready to create our beta Trending API models. 

<!-- TODO: Add a slide that describes what this tool is and why we use it. Add a demo slide, we'll take questions after the demo. -->

## Step 2. Generate and build the beta model class library

This solution is setup with existing references to the Microsoft Graph client library and JSON.Net. The reason why we need this is because the generated class library models contain references to classes in these libraries. This solution has been targeted at both .Net 4.5 and .NetStandard 1.1.

This solution contains the following branches:
* master - contains the initial state of this solution. This way you can use this solution as a starting point to add your own beta classes.
* final - contains the final state of the Trending beta class library. Use this branch to compare the results of you following the steps in this readme to the results we get after we walked through these steps.

This solution contains two metadata files:
* metadataTrending.xml - contains the metadata used for generating the Trending API models.
* metadataSkeleton.xml - contains a basic skeleton and instructions for what you need to generate a beta class library.

Now let's take a look at this solution.

1. Clone this repo:

        git clone https://github.com/MIchaelMainer/msgraph-sdk-dotnet-beta.git

2. Open this solution and look over the contents of the metadataSkeleton.xml and metadataTrending.xml files. You'll want to alter one of these for the beta models that you'll use in your application. 
3. Run our generation tool, vipr.exe, to create our beta models:

        vipr.exe C:\repos\msgraph-sdk-dotnet-beta\msgraph-sdk-dotnet-beta-models\metadataTrending.xml --writer="GraphODataTemplateWriter" --output=C:\repos\msgraph-sdk-dotnet-beta\msgraph-sdk-dotnet-beta-models\Generated

You can find vipr.exe in a path like: C:\repos\MSGraph-SDK-Code-Generator\src\GraphODataTemplateWriter\bin\Debug\Vipr.exe. The first argument is the input metadata. The second argument named **writer** is always the same: GraphODataTemplateWriter. The third argument named **output** is the output location for the generated files. We are using the location in msgraph-sdk-dotnet-beta in this example. We suggest that you do the same.

At this point, you should now see models and requests in the *Generated* folder. Delete the requests folder as we won't be using the request builder classes. Delete entity.cs in the models folder as the Microsoft Graph client library already contains this file.

4. Check that we are building this class library with the same referenced version of the Microsoft Graph client library used in our target application. Otherwise we'd have to use a binding redirect. 
5. Build this solution (F6).

Now we have generated the class library code files and built the class library. We are now ready to add this library to our existing solution.

<!-- TODO: Add a slide that describes what we will do in this step: clone this repo, what is the graph metadata, our metadata in this repo. Add a demo slide, we'll take questions after the demo. -->

## Step 3. Integrate beta model class library into the console-csharp-connect-sample

Now that we have Trending API models, we can add them to an existing Microsoft Graph client library solution. 

1. Clone the forked [console-csharp-connect-sample](https://github.com/MIchaelMainer/console-csharp-connect-sample) sample. The **master** branch will be our starting point. If you want to jump ahead or compare, the **final** branch contains the final state of this solution.
2. Get a clientId (aka appId) and add it to the [Constants.cs](https://github.com/MIchaelMainer/console-csharp-connect-sample/blob/master/console-csharp-connect-sample/Constants.cs#L14) file. See the ReadMe in the [console-csharp-connect-sample](https://github.com/MIchaelMainer/console-csharp-connect-sample) for instructions on how to do this. Make sure that you select your application registration as a native app.
3. Add the Sites.Read.All and/or Sites.ReadWrite.All scopes to the Scopes variable in AuthenticationHelper.cs. This should be on or near [line 22](https://github.com/MIchaelMainer/console-csharp-connect-sample/blob/master/console-csharp-connect-sample/AuthenticationHelper.cs#L22). 
4. Add a reference to the beta model class library we created. This was Microsoft.Graph.Beta.Models.dll.
5. Give Microsoft.Graph.Beta.Models.dll an alias called `GraphBetaModels` so that we can reference the objects in that library. You can do this in the Microsoft.Graph.Beta.Models reference property menu or directly in the csproj file reference within an `<Aliases/>` element. You'll replace the value `global` with `GraphBetaModels`. We do this since the Microsoft.Graph client library and our beta model class library are in the same namespace. We'll use this to add our extern alias directive.
6. Add `extern alias GraphBetaModels;` to the top of Program.cs above all other directives. Now we can reference the objects in our model class library.
7. Starting at [line 40](https://github.com/MIchaelMainer/console-csharp-connect-sample/blob/master/console-csharp-connect-sample/Program.cs#L40) in Program.cs, add the following code (you can also get this code in a Visual Studio snippet file included with the [console-csharp-connect-sample](https://github.com/MIchaelMainer/console-csharp-connect-sample) repo):

```csharp
// 1. Create request message with the URL for the trending API.
string requestUrl = "https://graph.microsoft.com/beta/me/insights/trending";
HttpRequestMessage hrm = new HttpRequestMessage(HttpMethod.Get, requestUrl);

// 2. Authenticate (add access token) our HttpRequestMessage
graphClient.AuthenticationProvider.AuthenticateRequestAsync(hrm).GetAwaiter().GetResult();

// 3. Send the request and get the response.
HttpResponseMessage response = graphClient.HttpProvider.SendAsync(hrm).Result;

// 4. Get the trending response.
var content = response.Content.ReadAsStringAsync().Result;
JObject trendingResponseBody = JObject.Parse(content);

// 4. Get the array of trending objects from the 'value' key.
JToken arrayOfTrendingObjects = trendingResponseBody.GetValue("value");

// 5. Get a list of Trending objects.
List<GraphBetaModels.Microsoft.Graph.Trending> trendingList = new List<GraphBetaModels.Microsoft.Graph.Trending>();

// 5. Deserialize each trending object.
foreach (JToken t in arrayOfTrendingObjects.Children())
{
    GraphBetaModels.Microsoft.Graph.Trending trendingObj = graphClient.HttpProvider
                                                                      .Serializer
                                                                      .DeserializeObject<GraphBetaModels.Microsoft
                                                                                                        .Graph
                                                                                                        .Trending>(t.ToString());
    trendingList.Add(trendingObj);
}

// 6. Access the contents of the trending objects from the model.
foreach (GraphBetaModels.Microsoft.Graph.Trending trendingItem in trendingList)
{
    Console.WriteLine($"Trending id: {trendingItem.Id}");
    Console.WriteLine($"Trending resource title: {trendingItem.ResourceVisualization.Title}");
    Console.WriteLine($"Trending resource preview text: {trendingItem.ResourceVisualization.PreviewText}");
    Console.WriteLine($"Trending resource web url: {trendingItem.ResourceReference.WebUrl}\n");
}
```
8. Press F5 and run the updated application to get the beta Trending information.

<!-- TODO: Add a slide that describes what we will do in this step:  Add a demo slide, we'll take questions after the demo. -->

## Summary

The Microsoft Graph is changing often so we don't provide a beta client library. Still, people want to use the beta functionality in their application. This workflow shows how you integrate that beta functionality into your existing v1.0 Graph client. The main benefits of this are that you get to:
* use beta objects in your existing application.
* use your existing authenticated Graph client.
* use the existing serializer to serialize/deserialize Microsoft Graph objects.

I see the following opportunities to further this:
* script this workflow so that metadata can be autogenerated based on target entities.
* update this workflow so that we can get the *CollectionPage objects generated so that we no longer have to do work with the JSON to get a collection of objects.