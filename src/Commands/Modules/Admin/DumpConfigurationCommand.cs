using System.Threading.Tasks;
using Qmmands;
using Volte.Commands.Results;
using Volte.Core.Entities;
using Volte.Core.Helpers;

namespace Volte.Commands.Modules
{
    public partial class AdminModule
    {
        [Command("DumpConfig", "Dump", "Dc")]
        [Description("Dumps this guild's configuration to paste.greemdev.net in JSON format for getting support.")]
        [Remarks("dumpconfig")]
        [RequireGuildAdmin]
        public async Task<ActionResult> DumpConfigurationAsync()
        {
            return Ok(await HttpHelper.PostToGreemPasteAsync(Context.GuildData.ToString(), Context.Services));
        }
    }
}