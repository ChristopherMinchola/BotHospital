using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace BotHospital.Dialogs
{
    public class RootDialog: ComponentDialog
    {
        public string userName { get; set; }
        public int userAge { get; set; }
        public string userCommand { get; set; }


        public RootDialog() {

            var waterfallStep = new WaterfallStep[]
            {
                SetName,
                SetAge,
                GetWhatToDo,
                TryUserCommand,
                CreateApointment,
                SendApointment,

            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallStep));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>), ValidateAge));
        }

        private async Task<bool> ValidateAge(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
        {
            return await Task.FromResult(
                promptContext.Recognized.Succeeded &&
                promptContext.Recognized.Value > 0 &&
                promptContext.Recognized.Value < 150
                );
        }

        private async Task<DialogTurnResult> SetName(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string dayPhase = BotUtils.GetDayPhase();
            await stepContext.Context.SendActivityAsync($"{dayPhase}, para iniciar una conversación, necesitaré saber algunos datos sobre ti 👨‍🔬", cancellationToken: cancellationToken);
            await Task.Delay(1000);
            return await stepContext.PromptAsync(
                nameof(TextPrompt),
                new PromptOptions { Prompt = MessageFactory.Text("Asi que dime, ¿Cual es tu nombre?")},
                cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> SetAge(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            userName = (string)stepContext.Result;
            return await stepContext.PromptAsync(
                nameof(NumberPrompt<int>),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text($"Mucho gusto {userName}, ¿Cuantos años tienes?"),
                    RetryPrompt = MessageFactory.Text($"{userName}, no creo que esa sea tu edad, o ¿si? Recuerda usar solo números")
                }, cancellationToken
                );
        }

        private async Task<DialogTurnResult> GetWhatToDo(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            userAge = (int)stepContext.Result;

            if (userAge < 18)
            {
                await stepContext.Context.SendActivityAsync($"Lo siento {userName}, pero no puedo atenderte, eres menor de edad, necesitas la ayuda de tus padres o tutor.", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }

            return await stepContext.PromptAsync(
                nameof(TextPrompt),
                new PromptOptions { Prompt = MessageFactory.Text($"Excelente {userName}, asi que dime, ¿Que puedo hacer por ti? 👨‍🔬") },
                cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> TryUserCommand (WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            userCommand = (string)stepContext.Result;
            if(BotUtils.ContainsWord(userCommand, new System.Collections.Generic.List<string> { "cita", "consulta", "doctor", "medico", "especialista" }))
            {
                return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions { Prompt = MessageFactory.Text($"Claro {userName}, te agendaré una cita con un especialista, por favor dime, ¿Que especialidad necesitas? \n" +
                    $"- Dermatología 🧑‍🦰\n" +
                    $"- Oftalmología 👁️\n" +
                    $"- Cardiología 💖\n" +
                    $"- Rehabilitación 🤕\n" +
                    $"- Psiquiatría 🧠") },
                    cancellationToken: cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync($"Lo siento {userName}, no entiendo lo que me pides.", cancellationToken: cancellationToken);
                await stepContext.Context.SendActivityAsync($"Por el momento solo puedo agendar citas, pronto de podré ayudar con mas cosas, gracias por tu paciencia.", cancellationToken: cancellationToken);
                string dayPhase = BotUtils.GetDayPhase();
                await stepContext.Context.SendActivityAsync($"Hasta la proxima, {dayPhase}.", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> CreateApointment (WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            List<string> hospitalSpecialties = new List<string>();

            hospitalSpecialties.Add("dermatología");
            hospitalSpecialties.Add("oftalmología");
            hospitalSpecialties.Add("cardiología");
            hospitalSpecialties.Add("rehabilitación");
            hospitalSpecialties.Add("psiquiatría");

            string userSpecialty = BotUtils.GetSimilarString((string)stepContext.Result, hospitalSpecialties);

            if (!string.IsNullOrEmpty(userSpecialty))
            {
                await stepContext.Context.SendActivityAsync($"Claro {userName}, te agendaré una cita con un especialista en {userSpecialty}", cancellationToken: cancellationToken);
                await stepContext.Context.SendActivityAsync($"Necesito saber tu correo para enviarte la información de tu cita por ahi, asi que", cancellationToken: cancellationToken);
                return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions { Prompt = MessageFactory.Text($"¿Cual es tu correo?") },
                    cancellationToken: cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync($"Lo siento {userName}, no entiendo lo que me pides. Creo que no contamos con esa especialidad 😔", cancellationToken: cancellationToken);
            }
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> SendApointment(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string userEmail = BotUtils.GetEmailFromString((string)stepContext.Result);
            if(!string.IsNullOrEmpty(userEmail))
            {
                await stepContext.Context.SendActivityAsync($"Gracias {userName}, te enviaremos la información de tu cita a {userEmail}", cancellationToken: cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync($"Lo siento {userName}, ese no es un correo validado. ❌", cancellationToken: cancellationToken);
            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
