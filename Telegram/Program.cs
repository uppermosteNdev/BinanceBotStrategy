using System;
using Telegram;
using Telegram.Bot;

var botClient = new TelegramBotClient("5812115442:AAGeGPjXhx2iS7KURxQu9w20LTsNqSphx_I");

// Create a new bot instance
var metBot = new BotEngine(botClient);

// Listen for messages sent to the bot
await metBot.ListenForMessagesAsync();