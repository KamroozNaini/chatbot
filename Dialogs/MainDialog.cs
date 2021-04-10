// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly HRManagementRecognizer _luisRecognizer;
        protected readonly ILogger Logger;

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(HRManagementRecognizer luisRecognizer, GivingInformationDialog givingInformation, ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            _luisRecognizer = luisRecognizer;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(givingInformation);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var messageText = stepContext.Options?.ToString() ?? $"Welcome to HR Management Program, Lets start with knowing who you are, please enter your name ?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Call LUIS and gather any potential details. (Note the TurnContext has the response to the prompt.)
            var luisResult = await _luisRecognizer.RecognizeAsync<HRManagement>(stepContext.Context, cancellationToken);
            switch (luisResult.TopIntent().intent)
            {
                case HRManagement.Intent.GiveMyName:
                    var employeeInformation = new GivingInformationModel()
                    {
                        Name = luisResult.ToName,
                    };
                    await ShowWarningForUnsupportedNames(stepContext, luisResult, cancellationToken);
                    return await stepContext.BeginDialogAsync(nameof(GivingInformationDialog), employeeInformation, cancellationToken);
                    break;
                // handle the error or trap 
                default:
                    // Catch all for unhandled intents
                    return await ShowWarningForUnsupportedNames(stepContext, luisResult, cancellationToken);
                    break;
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        // Shows a warning if the requested From or To cities are recognized as entities but they are not in the Airport entity list.
        // In some cases LUIS will recognize the From and To composite entities as a valid cities but the From and To Airport values
        // will be empty if those entity values can't be mapped to a canonical item in the Airport.
        private async Task<DialogTurnResult> ShowWarningForUnsupportedNames(WaterfallStepContext stepContext, HRManagement luisResult, CancellationToken cancellationToken)
        {
            var  result = string.Empty;
            var message = new Activity();
            var fromEntities = luisResult.ToName;
            if (string.IsNullOrEmpty(fromEntities))
            {
                result = "the name you entered is not valid.";
                message = MessageFactory.Text(result, result, InputHints.IgnoringInput);
                await stepContext.Context.SendActivityAsync(message, cancellationToken);
                return await stepContext.ReplaceDialogAsync(InitialDialogId, "Lets start over, please enter your name", cancellationToken);
            }
            return null;
        }

        private async Task<DialogTurnResult> ShowWarningForUnsupportedCommand(WaterfallStepContext stepContext, HRManagement luisResult, CancellationToken cancellationToken)
        {
            var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try asking in a different way (intent was {luisResult.TopIntent().intent})";
            var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
            await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
            return await stepContext.ReplaceDialogAsync(InitialDialogId, "unpredictable error happens, please call the admin", cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // If the child dialog ("BookingDialog") was cancelled, the user failed to confirm or if the intent wasn't BookFlight
            // the Result here will be null.
            if (stepContext.Result is GivingInformationModel result)
            {
                // Now we have all the booking details call the booking service.

                // If the call to the booking service was successful tell the user.

                var timeProperty = new TimexProperty(result.Name);
                var travelDateMsg = timeProperty.ToNaturalLanguage(DateTime.Now);
                var messageText = $"I have you as a valid employee in my list {result.Name}";
                var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                await stepContext.Context.SendActivityAsync(message, cancellationToken);
            }

            // Restart the main dialog with a different message the second time around
            var promptMessage = "So what else I can do for you ?";
            return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
        }
    }
}
