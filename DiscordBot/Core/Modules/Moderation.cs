﻿using Discord.Commands;
using Discord;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Linq;
using SIVA.Core.Bot;
using System;

namespace SIVA.Core.Modules
{
    public class Moderation : ModuleBase<SocketCommandContext>
    {
        private DiscordSocketClient _client;

        public string Count = "";

        [Command("Ban")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task BanUser(SocketGuildUser user, [Remainder]string reason = "")
        {

            await Context.Guild.AddBanAsync(user, 7, reason: reason);
            var embed = new EmbedBuilder();
            embed.WithDescription(Utilities.GetFormattedLocaleMsg("BanText", user.Mention, Context.User.Mention));
            embed.WithFooter(Utilities.GetFormattedLocaleMsg("CommandFooter", Context.User.Username));
            embed.WithColor(new Color(Bot.Config.bot.DefaultEmbedColour));
            await ReplyAsync("", false, embed);

        }


        [Command("Softban"), Alias("Sb")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task BanThenUnbanUser(SocketGuildUser user)
        {
            var embed = new EmbedBuilder();
            await Context.Guild.AddBanAsync(user, 7);
            await Context.Guild.RemoveBanAsync(user);
            embed.WithDescription($"{Context.User.Mention} softbanned <@{user.Id}>, deleting the last 7 days of messages from that user.");
            embed.WithColor(Bot.Config.bot.DefaultEmbedColour);
            embed.WithFooter(Utilities.GetFormattedLocaleMsg("CommandFooter", Context.User.Username));
            await ReplyAsync("", false, embed);

        }

        [Command("IdBan")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task BanUserById(ulong userid, [Remainder]string reason = "")
        {
            if (reason == "")
            {
                reason = $"Banned by {Context.User.Username}#{Context.User.Discriminator}";
            }
            await Context.Guild.AddBanAsync(userid, 7, reason);
            var embed = new EmbedBuilder();
            embed.WithDescription(Utilities.GetFormattedLocaleMsg("BanText", $"<@{userid}>", $"<@{Context.User.Id}>"));
            embed.WithFooter(Utilities.GetFormattedLocaleMsg("CommandFooter", Context.User.Username));
            embed.WithColor(new Color(Bot.Config.bot.DefaultEmbedColour));

            await ReplyAsync("", false, embed);
        }

        [Command("Rename")]
        [RequireUserPermission(GuildPermission.ManageNicknames)]
        public async Task SetUsersNickname(SocketGuildUser user, [Remainder]string nick)
        {
            await user.ModifyAsync(x => x.Nickname = nick);
            var embed = new EmbedBuilder();
            embed.WithDescription($"Set <@{user.Id}>'s nickname on this server to **{nick}**!");
            embed.WithColor(Bot.Config.bot.DefaultEmbedColour);
            embed.WithFooter(Utilities.GetFormattedLocaleMsg("CommandFooter", Context.User.Username));
            await ReplyAsync("", false, embed);
        }

        [Command("Kick")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task KickUser(SocketGuildUser user, [Remainder]string reason = "")
        {

            await user.KickAsync();
            var embed = new EmbedBuilder();
            embed.WithDescription(Utilities.GetFormattedLocaleMsg("KickUserMsg", user.Mention, Context.User.Mention));
            embed.WithFooter(Utilities.GetFormattedLocaleMsg("CommandFooter", Context.User.Username));
            embed.WithColor(new Color(Bot.Config.bot.DefaultEmbedColour));
            await ReplyAsync("", false, embed);

        }

        /*[Command("ModLog")]
        public async Task SetModlogChannel()
        {
            var config = Modlogs.GetModlogConfig(Context.Guild.Id);
            if (config == null)
            {
                var newConfig = Modlogs.CreateModlogConfig(Context.Guild.Id, Context.Channel.Id);
                await ReplyAsync("Modlog config created and channel set to current channel!");
            }
            else
            {
                config.channelId = Context.Channel.Id;
                Modlogs.SaveModlogConfig();
                await ReplyAsync("Modlog channel set to current channel!");
            }
        }*/

        [Command("AddRole"), Alias("AR")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task GiveUserSpecifiedRole(SocketGuildUser user, [Remainder]string role)
        {
            var targetRole = user.Guild.Roles.FirstOrDefault(r => r.Name == role);

            var embed = new EmbedBuilder();
            embed.WithFooter(Utilities.GetFormattedLocaleMsg("CommandFooter", Context.User.Username));
            embed.WithColor(Bot.Config.bot.DefaultEmbedColour);
            embed.WithDescription(Utilities.GetFormattedLocaleMsg("AddRoleCommandText", role, user.Username + "#" + user.Discriminator));

            await user.AddRoleAsync(targetRole);
            await ReplyAsync("", false, embed);
        }

        [Command("RemRole"), Alias("RR")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task TakeAwaySpecifiedRole(SocketGuildUser user, [Remainder]string role)
        {
            var targetRole = user.Guild.Roles.FirstOrDefault(r => r.Name == role);

            var embed = new EmbedBuilder();
            embed.WithFooter(Utilities.GetFormattedLocaleMsg("CommandFooter", Context.User.Username));
            embed.WithColor(Bot.Config.bot.DefaultEmbedColour);
            embed.WithDescription(Utilities.GetFormattedLocaleMsg("RemRoleCommandText", role, user.Username + "#" + user.Discriminator));

            await user.RemoveRoleAsync(targetRole);
            await ReplyAsync("", false, embed);
        }

        [Command("Purge")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task PurgeMessages(int amount)
        {
            if (amount < 1)
            {
                await ReplyAsync("You cannot delete less than 1 message.");
            }
            else
            {
                var messages = await Context.Channel.GetMessagesAsync(amount).Flatten();
                await Context.Channel.DeleteMessagesAsync(messages);
            }
        }

        [Command("Warn")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task WarnUser(SocketGuildUser user, [Remainder]string reason)
        {
            var embed = new EmbedBuilder();
            embed.WithDescription(Utilities.GetFormattedLocaleMsg("WarnCommandText", user.Mention, reason));
            embed.WithFooter(Utilities.GetFormattedLocaleMsg("CommandFooter", Context.User.Username));
            embed.WithColor(Bot.Config.bot.DefaultEmbedColour);
            var ua = UserAccounts.UserAccounts.GetAccount(user);
            ua.Warns.Add(reason);
            ua.WarnCount = (uint)ua.Warns.Count;
            UserAccounts.UserAccounts.SaveAccounts();
            await ReplyAsync("", false, embed);

        }

        [Command("ClearWarns"), Alias("CW")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ClearUsersWarns(SocketGuildUser user)
        {
            var ua = UserAccounts.UserAccounts.GetAccount(user);
            var embed = new EmbedBuilder();
            embed.WithDescription($"{ua.WarnCount} warn(s) cleared for {user.Mention}");
            embed.WithFooter(Utilities.GetFormattedLocaleMsg("CommandFooter", Context.User.Username));
            embed.WithColor(Bot.Config.bot.DefaultEmbedColour);
            ua.WarnCount = 0;
            ua.Warns.Clear();
            UserAccounts.UserAccounts.SaveAccounts();
            
            await ReplyAsync("", false, embed);

        }

        [Command("Warns"), Priority(0)]
        public async Task WarnsAmountForGivenUser()
        {
            var embed = new EmbedBuilder();
            var ua = UserAccounts.UserAccounts.GetAccount(Context.User);
            Count = ua.WarnCount == 1 ? "WarnsSingulText" : "WarnsPluralText";
            embed.WithFooter(Utilities.GetFormattedLocaleMsg("CommandFooter", Context.User.Username));
            embed.WithColor(Bot.Config.bot.DefaultEmbedColour);
            embed.WithDescription($"{Context.User.Mention} has {ua.WarnCount} warns and their most recent warn is `{ua.Warns.Last()}`");
            await ReplyAsync("", false, embed);

        }

        [Command("Warns"), Priority(1)]
        public async Task WarnsAmountForGivenUser(SocketGuildUser user)
        {
            var embed = new EmbedBuilder();
            var ua = UserAccounts.UserAccounts.GetAccount(user);
            if (ua.WarnCount == 1)
            {
                Count = "WarnsSingulText";
            }
            else
            {
                Count = "WarnsPluralText";
            }
            embed.WithFooter(Utilities.GetFormattedLocaleMsg("CommandFooter", Context.User.Username));
            embed.WithColor(Bot.Config.bot.DefaultEmbedColour);
            embed.WithDescription($"{user.Mention} has {ua.WarnCount} warns and their most recent warn is `{ua.Warns.Last()}`");

            await ReplyAsync("", false, embed);

        }
    }
}