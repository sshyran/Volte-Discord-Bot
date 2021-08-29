using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Gommon;
using Volte.Core.Entities;
using Volte.Core.Helpers;
using Volte.Services;

namespace Volte.Commands.Interaction.Commands
{
        public sealed class ReminderCommand : ApplicationCommand
    {
        public ReminderCommand() : base("reminder", "Create, view & delete reminders.") { }

        public override async Task HandleSlashCommandAsync(SlashCommandContext ctx)
        {
            var reply = ctx.CreateReplyBuilder(true);
            var subcommand = ctx.Options.First().Value;
            var timeFromNow = subcommand.GetOptionOfValue<string>("time-from-now");
            var reminderContent = subcommand.GetOptionOfValue<string>("content");
            if (subcommand.Name is "create")
            {
                var timeSpanRes = await ctx.Services.Get<CommandsService>()
                    .GetTypeParser<TimeSpan>().ParseAsync(timeFromNow);
                if (timeSpanRes.IsSuccessful)
                {
                    var end = DateTime.Now.Add(timeSpanRes.Value);
                    ctx.Db.CreateReminder(Reminder.CreateFrom(ctx, end, reminderContent));
                    reply.WithEmbeds(ctx.CreateEmbedBuilder($"I'll remind you about {Format.Code(reminderContent)}.")
                        .WithTitle($"In {end.GetDiscordTimestamp(TimestampType.Relative)},"));
                }
                else
                {
                    reply.WithEmbed(eb => eb.WithTitle(timeSpanRes.FailureReason));
                }
            }
            else
            {
                var reminders = ctx.Db.GetReminders(ctx.User.Id);
                if (reminders.IsEmpty())
                    reply.WithEmbed(e => e.WithTitle("You don't have any reminders."));
                else
                {
                    reply.WithSelectMenu(new SelectMenuBuilder()
                            .WithCustomId("reminder:menu")
                            .WithOptions(reminders.Take(25)
                                .Select(r =>
                                {
                                    var truncated = r.ReminderText.Length > 25;
                                    return new SelectMenuOptionBuilder()
                                        .WithLabel(
                                            $"{r.Id}: {(truncated ? $"{r.ReminderText.Take(25).Select(x => x.ToString()).Join("")}..." : r.ReminderText)}")
                                        .WithValue(r.Id.ToString());
                                })
                                .ToList())
                            .WithPlaceholder("Choose a reminder"))
                        .WithEmbed(eb => eb.WithTitle("Select a reminder below to proceed."));
                }
            }

            await reply.RespondAsync();
        }

        private readonly ButtonBuilder _deleteButton = ButtonBuilder.CreatePrimaryButton("Delete reminder",
            "reminder:delete", DiscordHelper.X.ToEmoji());

        private readonly Func<IEnumerable<Reminder>, SelectMenuBuilder> _getReminderMenu = rs
            => new SelectMenuBuilder()
                .WithCustomId("reminder:menu")
                .WithOptions(rs.Take(25)
                    .Select(r 
                        => new SelectMenuOptionBuilder()
                            .WithLabel($"{r.Id}: {(r.ReminderText.Length > 25 ? $"{r.ReminderText.Take(25).Select(x => x.ToString()).Join("")}..." : r.ReminderText)}")
                            .WithValue(r.Id.ToString()))
                    .ToList())
                .WithPlaceholder("Choose a reminder");

        public override async Task HandleComponentAsync(MessageComponentContext ctx)
        {
            switch (ctx.CustomIdParts[1])
            {
                case "delete":
                    var reply = ctx.CreateReplyBuilder(true);
                    var fields = ctx.Backing.Message.Embeds.First().Fields;
                    if (fields.IsEmpty())
                        reply.WithEmbed(x => x.WithTitle("Please select a reminder from the list!"));
                    else
                    {
                        var reminderId = long.Parse(
                            ctx.Backing.Message.Embeds.First().Fields.First(f => f.Name is "Unique ID").Value);
                        var targetReminder = ctx.Db.GetReminder(reminderId);

                        if (ctx.Db.TryDeleteReminder(targetReminder))
                            await ctx.Backing.UpdateAsync(x =>
                            {
                                var reminders = ctx.Db.GetReminders(ctx.User.Id);
                                if (reminders.IsEmpty())
                                {
                                    x.Components = new ComponentBuilder().Build();
                                    x.Embed = ctx.CreateEmbedBuilder()
                                        .WithTitle("You've deleted all of your reminders.").Build();
                                }
                                else
                                {
                                    x.Components = new ComponentBuilder()
                                        .WithSelectMenu(_getReminderMenu(ctx.Db.GetReminders(ctx.User.Id)))
                                        .Build();
                                    x.Embed = ctx.CreateEmbedBuilder()
                                        .WithTitle("Please select a reminder from the list.").Build();
                                }

                            });
                        else
                            await reply.WithEmbedFrom("Reminder couldn't be deleted").RespondAsync();
                        
                        return;
                    }

                    await reply.RespondAsync();
                    break;
                case "menu":
                    var reminders = ctx.Db.GetReminders(ctx.User.Id);
                    var reminder = reminders.First(x => x.Id == long.Parse(ctx.Backing.Data.Values.First()));
                    await ctx.Backing.UpdateAsync(x =>
                    {
                        x.Embed = ctx.CreateEmbedBuilder()
                            .WithTitle(reminder.TargetTime.GetDiscordTimestamp(TimestampType.Relative))
                            .AddField("Unique ID", reminder.Id)
                            .AddField("Reminder", Format.Code(reminder.ReminderText))
                            .AddField("Created", reminder.CreationTime.GetDiscordTimestamp(TimestampType.LongDateTime))
                            .Build();
                        x.Components = new ComponentBuilder()
                            .WithButton(_deleteButton)
                            .WithSelectMenu(_getReminderMenu(reminders))
                            .Build();
                    });
                    break;
            }
        }


        public override SlashCommandBuilder GetSlashBuilder(IServiceProvider provider)
            => new SlashCommandBuilder()
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("create")
                    .WithDescription("Create a reminder.")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("time-from-now")
                        .WithDescription("When do you want to be reminded? i.e. 2d4h, 2 days 4 hours.")
                        .WithType(ApplicationCommandOptionType.String)
                        .WithRequired(true))
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("content")
                        .WithDescription("What do you want to reminded of?")
                        .WithType(ApplicationCommandOptionType.String)
                        .WithRequired(true)))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("view")
                    .WithDescription("View and/or delete reminders; selectable via dropdown menu.")
                    .WithType(ApplicationCommandOptionType.SubCommand));
    }
}