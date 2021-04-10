// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading;
using System.Threading.Tasks;
using CoreBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class GivingInformationDialog : CancelAndHelpDialog
    {
        private const string ThxProvidingYourName = "Thanks for providing your name";
        private const string CodeValidationExplenation  = "Next we would like to validate your account by your employee number";
        private const string EnterYourCode = " Please enter your employee code?";
        private const string CodeIsNotValidReTryAgain = "The code you enter is not valid, can you please renter the code again ";
        private const string CodeIsValid = "Thanks we validated you ?";
        private const string whatCanIdoForYou = "So tell me what can I do for you, (ex : booking vacations, sick days, leave day?";
        public GivingInformationModel informationModel = new GivingInformationModel();
        public GivingInformationDialog()
            : base(nameof(GivingInformationDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new DateResolverDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                AskNameStepAsync,
                AskCodeStepAsync,
                ValidateEmployeeStepAsync,
                BookVacation,
                ProcessVacation,
                Acknowledgement,
                AcknowledgementConfirmation
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AskNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var employeeInformation = (GivingInformationModel)stepContext.Options;
            if (employeeInformation.Name == null)
            {
                var provideName = MessageFactory.Text("Please provide your name", "", InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions {Prompt = provideName},
                    cancellationToken);
            }

            return await stepContext.NextAsync(employeeInformation.Name, cancellationToken);
        }

        private async Task<DialogTurnResult> AskCodeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var employeeInformation = (GivingInformationModel)stepContext.Options;
            employeeInformation.Name = (string)stepContext.Result;
            if (employeeInformation.EmployeeCode == null)
            {
                var provideCode = MessageFactory.Text(ThxProvidingYourName + " " + employeeInformation.Name + "."+ CodeValidationExplenation + " " + employeeInformation.Name +"."+ EnterYourCode, "",
                    InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions {Prompt = provideCode},
                    cancellationToken);
            }
            return await stepContext.NextAsync(employeeInformation.EmployeeCode, cancellationToken);
        }

        private async Task<DialogTurnResult> ValidateEmployeeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var employeeInformation = (GivingInformationModel)stepContext.Options;
            employeeInformation.EmployeeCode = (string)stepContext.Result;
            //
            if (!employeeInformation.IsValidated)
            {
                //if ((employeeInformation.Name == "kamrooz") && ((employeeInformation.EmployeeCode != "1230")))
                //{
                //    var notValidCode = MessageFactory.Text("We checked the database, the code you provided is not valid for " + employeeInformation.Name +
                //                                           ".Please enter your Employee number. Thanks",
                //        InputHints.ExpectingInput);
                //    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = notValidCode },
                //        cancellationToken);
                //}
            }
            return await stepContext.NextAsync(employeeInformation.EmployeeCode, cancellationToken);
        }

        private async Task<DialogTurnResult> BookVacation(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var vacation = (GivingInformationModel)stepContext.Options;
            vacation.EmployeeCode = (string)stepContext.Result;
            if (vacation.ActivityName == null)
            {
                var provideCode = MessageFactory.Text("Thanks, awesome, Now you are validated, So tell us as your HR manager. How we can assist you today ? ",
                    InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = provideCode },
                    cancellationToken);
            }
            return await stepContext.NextAsync(vacation.ActivityName, cancellationToken);
        }

        private async Task<DialogTurnResult> ProcessVacation(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var vacation = (GivingInformationModel)stepContext.Options;
            vacation.EmployeeCode = (string)stepContext.Result;
            if (vacation.ActivityName == null)
            {
                vacation.ActivityName = "vacation";
                var provideCode = MessageFactory.Text("so for how many days you want to go for vacation " + vacation.Name + " ? " , InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = provideCode },
                    cancellationToken);
            }
            return await stepContext.NextAsync(vacation.Duration, cancellationToken);
        }

        private async Task<DialogTurnResult> Acknowledgement(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var vacation = (GivingInformationModel)stepContext.Options;
            vacation.Duration = (string)stepContext.Result;
            if (vacation.Duration != null )
            {
                // insert into database 
                var provideCode = MessageFactory.Text("So just to confirm you [" + vacation.Name + "] are going for vacation for [" + vacation.Duration + "]. Please type yes for confirmation or no for cancel the request",
                    InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = provideCode },
                    cancellationToken);
            }
            return await stepContext.NextAsync(vacation.Confirmed, cancellationToken);
        }

        private async Task<DialogTurnResult> AcknowledgementConfirmation(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var vacation = (GivingInformationModel)stepContext.Options;
            vacation.Confirmed = (string)stepContext.Result;
            if (vacation.Confirmed != null)
            {
                // insert into database 
                //dataBase
                //
                var provideCode = MessageFactory.Text("Done. All set  [" + vacation.Name + "]. You booked for [" + vacation.Duration + "]. Please stay safe. Thanks for being patient.",
                    InputHints.ExpectingInput);
                 await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = provideCode },
                    cancellationToken);
                var provideCodeAnotherRequest = MessageFactory.Text("Anything else I can do for your day [" + vacation.Name + "].?",
                    InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = provideCodeAnotherRequest },
                    cancellationToken);
            }
            return await stepContext.NextAsync(vacation.ActivityName, cancellationToken);
        }
    }
}
