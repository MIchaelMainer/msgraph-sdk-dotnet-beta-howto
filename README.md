# msgraph-sdk-dotnet-beta

This repo contains an example class library solution and workflow for generating beta Microsoft Graph models. It shows how to use the beta models in a solution that already uses the Microsoft Graph client library. This workflow will show you how to generate a beta metadata file for the beta Trending API, C# model code files for the beta Trending API, and then show how you can integrate the beta Trending API objects in an existing .Net Microsoft Graph solution.

## About the msgraph-sdk-dotnet-beta.sln

This solution is setup with existing references to the Microsoft Graph client library and JSON.Net. The reason why we need this is because the generated client library models contain references to classes in these libraries. This solution has been targeted at both .Net 4.5 and .NetStandard 1.1.

This solution contains the following branches:
* master - contains the initial state of this solution. This way you can use this solution as a starting point to add your own beta classes.
* final - contains the final state of the Trending beta class library. Use this branch to compare the results of you following the steps in this readme to the results we get after we walked through these steps.

This solution contains two metadata files:
* metadataTrending.xml - contains the metadata used for generating the Trending API models.
* metadataSkeleton.xml - contains a basic skeleton of what you need to generate a beta class library.

## Create our custom beta metadata with the target Trending API models

We used the [Trending API documentation](https://developer.microsoft.com/en-us/graph/docs/api-reference/beta/api/insights_list_trending) to discover what the [Trending resource](https://developer.microsoft.com/en-us/graph/docs/api-reference/beta/resources/insights_trending) looks like and then found that resource (aka entity) in the [beta metadata](https://graph.microsoft.com/beta/$metadata). From the beta metadata, we discovered which complex types we needed to include in our Trending API metadata and added those. 

1. Clone this repo and open it in Visual Studio.
2. Open metadataSkeleton.xml and review the instructions in the XML comments. You'll need to fill out this skeleton with the models you want to use in your application.
3. Open metadataTrending.xml and review what we changed to make it work for the Trending API. 

At this point, we have a metadata file that contains the Trending API entity. 

<!-- TODO: I think this section needs more explanation. Depending on how many models someone wants to generate, and how complex the models are, this could be very confusing.  -->

Now we are ready to generate the Trending API models.

## Generate the Trending API beta models

We use the https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator to generate our code files. It has a submodule called VIPR that loads the metadata into memory and parses out the relationships between metadata objects into an object model that is understood by the code generator and its templates.

1. Recursively clone the repo: 

        git clone --recursive https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator.git

2. Build the solution.
3. Check that we are targeting C#. Open GraphODataTemplateWriter/.config/TemplateWriterSettings.json and check that the **TargetLanguage** is set to `CSharp`.
4. Generate our beta models. We'll give it the path to our metadata file and the Generated directory in our msgraph-sdk-dotnet-beta repo. vipr.exe will be found in \MSGraph-SDK-Code-Generator\src\GraphODataTemplateWriter\bin\Debug. Run the following from the command line or Visual Studio:

        vipr.exe D:\repos\msgraph-sdk-dotnet-beta\msgraph-sdk-dotnet-beta-models\metadataTrending.xml --writer="GraphODataTemplateWriter" --output=D:\repos\msgraph-sdk-dotnet-beta\msgraph-sdk-dotnet-beta-models\Generated

At this point, we have our beta models. Let's go back to msgraph-sdk-dotnet-beta-models project in Visual Studio to see what has been generated.

## Build our beta Trending API model class library

Our msgraph-sdk-dotnet-beta solution now has the generated models included in it if you set the output path correctly when you used the code generator. We are only concerned with the models. We will ignore the generated request builders since we won't be using them.

Now build this solution to create the Trending API model class library.

## How to update the console-csharp-connect-sample to use the Trending API beta models

Now that we have Trending API models, we can add them to an existing Microsoft Graph client library solution. 

1. Clone the forked [console-csharp-connect-sample](https://github.com/MIchaelMainer/console-csharp-connect-sample) sample. The **master** branch will be our starting point. If you want to jump ahead or compare, the **final** branch contains the final state of this solution. Here's a [pull request for comparing](https://github.com/MIchaelMainer/console-csharp-connect-sample/pull/3/files) what is in the **final** branch versus **master**.
2. Get a clientId and add it to the [Constants.cs](https://github.com/MIchaelMainer/console-csharp-connect-sample/blob/master/console-csharp-connect-sample/Constants.cs#L14) file. See the ReadMe in the [console-csharp-connect-sample](https://github.com/MIchaelMainer/console-csharp-connect-sample) for instructions on how to do this.
3. Add the Sites.Read.All and/or Sites.ReadWrite.All scopes to the Scopes variable in AuthenticationHelper.cs. This should be on or near [line 22](https://github.com/MIchaelMainer/console-csharp-connect-sample/blob/master/console-csharp-connect-sample/AuthenticationHelper.cs#L22). 
4. Add a reference to the beta model class library we created. This was Microsoft.Graph.Beta.Models.dll.
5. Give Microsoft.Graph.Beta.Models.dll an alias called `GraphBetaModels` so that we can reference the objects in that dll. You can do this in the Microsoft.Graph.Beta.Models reference property menu or directly in the csproj file reference within an `<Aliases/>` element. We do this since the Microsoft.Graph client library and our beta model class library are in the same namespace. We'll use this to add our extern alias directive.
6. Add `extern alias GraphBetaModels;` to the top of Program.cs above all other directives. Now we can reference the objects in our model class library.
7. Starting at [line 40](https://github.com/MIchaelMainer/console-csharp-connect-sample/blob/master/console-csharp-connect-sample/Program.cs#L40) in Program.cs, add the following code (you can also get this code in a Visual Studio snippet file included with the [console-csharp-connect-sample](https://github.com/MIchaelMainer/console-csharp-connect-sample) repo:

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

## Summary

The Microsoft Graph is changing often so we don't provide a beta client library. Still, people want to use the beta functionality in their application. This workflow shows how you integrate that beta functionality into your existing v1.0 Graph client. The main benefits of this are that you get to:
* use beta objects in your existing application.
* use your existing authenticated Graph client.
* use the existing serializer to serialize/deserialize Microsoft Graph objects.

I see the following opportunities to further this:
* script this workflow so that metadata can be autogenerated based on target entities.
* update this workflow so that we can get the *CollectionPage objects generated so that we no longer have to do work with the JSON to get a collection of objects.