using GovGuideBot;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotExperiments
{
    class Program
    {
        static ITelegramBotClient bot = new TelegramBotClient("6202454519:AAFLc_XRZKF3Be4Qkk1kAiuygUkIdKdR7HA");
        static string ipTarget = "http://192.168.43.33:2323/api/";
        static string questions = "chat/query";
        static string instructions = "instruction/document/";
        static string instructionsByCategory = "instruction/get/by/category/";
        static string categories = "category/get/all";
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Type == UpdateType.Message)
                {
                    var date = update.Message!.Date;
                    var datemes = DateTime.UtcNow - (date);
                    await Console.Out.WriteLineAsync(datemes.Seconds + "___seconds");
                    if (datemes.Seconds > 30)
                        return;

                    var message = update.Message;
                    var requestMessage = message.Text.ToLower();
                    Console.WriteLine(requestMessage);

                    switch (requestMessage)
                    {
                        case "/start":
                            await botClient.SendTextMessageAsync(message.Chat, "Приветствуем вас, вы можете задать мне вопрос или перейти во вкладку Категории чтобы изучить другие!\n" +
                                "Саламатсызбы, биздин тиркемеге кош келдиниз. Суроо берсениз болот же Категорияларды тандап изилдесениз болот");

                            var keyboardButtons = new[]
                            {
                                new KeyboardButton("Категории")
                            };

                            var replyKeyboardMarkup = new ReplyKeyboardMarkup(keyboardButtons);
                            replyKeyboardMarkup.ResizeKeyboard = true;
                            await bot.SendTextMessageAsync(message.Chat, "Choose an option:", replyMarkup: replyKeyboardMarkup);
                            return;
                        case "категории":
                            await ShowAllCategories(message.Chat);
                            return;
                        default:
                            var httpClient = new HttpClient();
                            Request request = new Request();
                            request.query = requestMessage;

                            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("utf-8")); // Specify UTF-8 encoding

                            var serializedRequest = JsonConvert.SerializeObject(request);

                            var httpContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json");
                            var response = await httpClient.PostAsync(ipTarget + questions, httpContent);
                            var responseContent = await response.Content.ReadAsStringAsync();
                            var responseToUserGPT = JsonConvert.DeserializeObject<GovGuideBot.ChatGPT.DataWrapper>(responseContent);
                            await botClient.SendTextMessageAsync(message.Chat, responseToUserGPT.Data.Answer);
                            if (responseToUserGPT.Data.Instructions.Length > 0)
                            {
                                List<InlineKeyboardButton[]> buttons = new List<InlineKeyboardButton[]>();
                                foreach (var item in responseToUserGPT.Data.Instructions)
                                {
                                    buttons.Add(new[] { new InlineKeyboardButton(item.Title) { Url = ipTarget + instructions + item.Id } });
                                }
                                var inlineKeyboard = new InlineKeyboardMarkup(buttons);
                                await bot.SendTextMessageAsync(message.Chat, "Можете перейти по ссылкам:\nТөмөнкү шилтемелерди бассаңыз болот:", replyMarkup: inlineKeyboard);
                            }
                            break;
                    }
                }

                if (update.Type == UpdateType.CallbackQuery)
                {
                    var date = update.CallbackQuery!.Message.Date;
                    var datemes = DateTime.UtcNow - (date);
                    if (datemes.Seconds > 30)
                        return;

                    var callbackQuery = update.CallbackQuery;
                    var data = callbackQuery.Data;
                    await ShowInstructions(data, callbackQuery.Message.Chat.Id);
                }
            }
            catch (Exception e)
            {
                await Console.Out.WriteLineAsync(e + "");
            }
        }
        public static async Task ShowAllCategories(ChatId chatId)
        {
            var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("utf-8")); // Specify UTF-8 encoding

            var response = await httpClient.GetAsync(ipTarget + categories);
            var responseContent = await response.Content.ReadAsStringAsync();

            var responseToUser = JsonConvert.DeserializeObject<GovGuideBot.Categories.DataWrapper>(responseContent);
            await Console.Out.WriteLineAsync(responseToUser.Data.Length + "categories____________Count");
            List<InlineKeyboardButton[]> buttonsCat = new List<InlineKeyboardButton[]>();

            foreach (var item in responseToUser.Data)
            {
                buttonsCat.Add(new[] { new InlineKeyboardButton(item.Name) { CallbackData = item.Id.ToString() } });
            }
            var inlineKeyboard = new InlineKeyboardMarkup(buttonsCat);
            await bot.SendTextMessageAsync(chatId, "Выберите категорию:\nКатегория тандаңыз:", replyMarkup: inlineKeyboard);
        }
        public static async Task ShowInstructions(string categoryId, ChatId chatId)
        {
            var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("utf-8")); // Specify UTF-8 encoding

            var response = await httpClient.GetAsync(ipTarget + instructionsByCategory + categoryId);
            var responseContent = await response.Content.ReadAsStringAsync();

            var responseToUser = JsonConvert.DeserializeObject<GovGuideBot.Instructions.DataWrapper>(responseContent);

            List<InlineKeyboardButton[]> buttonsCat = new List<InlineKeyboardButton[]>();
            foreach (var item in responseToUser.Data)
            {
                buttonsCat.Add(new[] { new InlineKeyboardButton(item.Title) { Url = ipTarget + instructions + item.Id.ToString() } });
            }
            await Console.Out.WriteLineAsync(buttonsCat.Count + "___________instructions");
            var inlineKeyboard = new InlineKeyboardMarkup(buttonsCat);
            await bot.SendTextMessageAsync(chatId, "Выберите инструкцию:\nИнструкцияны тандаңыз:", replyMarkup: inlineKeyboard);
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );

            Console.ReadLine();
        }
    }
}