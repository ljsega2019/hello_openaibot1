// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with EchoBot .NET Template version v4.17.1

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Azure;
using Azure.AI.OpenAI;
using static System.Environment;
using System;

namespace EchoBot.Bots
{
    public class EchoBot : ActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // var replyText = $"Echo: {turnContext.Activity.Text}";

            string endpoint = GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
            string key = GetEnvironmentVariable("AZURE_OPENAI_KEY");
            string system = GetEnvironmentVariable("SYSTEM_WHO");
            string tokens = GetEnvironmentVariable("TOKENS_LIMIT");
            string deployment = GetEnvironmentVariable("DEPLOYMENT_MODEL");

            string searchEndpoint = GetEnvironmentVariable("SEARCH_ENPOINT");
            string searchKey = GetEnvironmentVariable("SEARCH_KEY");
            string searchIndex = GetEnvironmentVariable("SEARCH_INDEX");

            OpenAIClient client = new(new Uri(endpoint), new AzureKeyCredential(key));

            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                Messages =
                {
                    new ChatMessage(ChatRole.System, system),
                    new ChatMessage(ChatRole.User, turnContext.Activity.Text),
                },
                AzureExtensionsOptions = new AzureChatExtensionsOptions()
                {
                    Extensions =
                    {
                        new AzureCognitiveSearchChatExtensionConfiguration()
                        {
                            SearchEndpoint = new Uri(searchEndpoint),
                            SearchKey = new AzureKeyCredential(searchKey),
                            IndexName = searchIndex,
                        },
                    }
                },
                MaxTokens = int.Parse(tokens)
            };


            Response<ChatCompletions> response = client.GetChatCompletions(deploymentOrModelName: deployment,chatCompletionsOptions);

            var replyText = response.Value.Choices[0].Message.Content;
            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
            

        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = GetEnvironmentVariable("WELCOME_TEXT");

            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }
}
