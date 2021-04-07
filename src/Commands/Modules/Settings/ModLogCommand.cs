using System.Threading.Tasks;
using Discord.WebSocket;
using Qmmands;
using Volte.Commands;

namespace Volte.Commands.Modules
{
    public sealed partial class SettingsModule
    {
        [Command("ModLog")]
        [Description("Sets the channel to be used for mod log.")]
        public Task<ActionResult> ModLogAsync([Description("The channel to be used by my moderation log.")] SocketTextChannel c)
        {
            Context.GuildData.Configuration.Moderation.ModActionLogChannel = c.Id;
            Db.Save(Context.GuildData);
            return Ok($"Set {c.Mention} as the channel to be used by mod log.");
        }
    }
}