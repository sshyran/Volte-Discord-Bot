﻿using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Qmmands;
using Volte.Core.Entities;
using Volte.Commands.Results;

namespace Volte.Commands.Modules
{
    public sealed partial class AdminUtilityModule
    {
        [Command("RoleColor", "RoleClr", "Rcl")]
        [Description("Changes the color of a specified role. Accepts a Hex or RGB value.")]
        [RequireBotGuildPermission(GuildPermission.ManageRoles)]
        public async Task<ActionResult> RoleColorAsync([Description("The role to modify.")] SocketRole role, [Remainder, Description("The color to change the role to. Accepts #hex and RGB.")] Color color)
        {
            await role.ModifyAsync(x => x.Color = color);
            return Ok($"Successfully changed the color of the role **{role.Name}**.");
        }
    }
}