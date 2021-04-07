using System;
using Volte.Commands;
using Volte.Commands.Modules;

namespace Volte.Core.Entities
{
    /// <summary>
    ///     Used on a base command of a command group; for Help command usage. Don't use this attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class DummyCommandAttribute : Attribute { }
    
    /// <summary>
    ///     Signals the <see cref="HelpModule"/> to list all available placeholders for welcome messages.
    ///     Don't use this on any other commands, unless they use the same placeholders.
    ///     Placeholders are defined in <see cref="WelcomeOptions"/> as a static property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ShowPlaceholdersInHelpAttribute : Attribute { }
    
    /// <summary>
    ///     Signals the <see cref="HelpModule"/> to show example time formats for use in Reminders.
    ///     Don't use this on any commands; unless they have a parameter of type <see cref="TimeSpan"/>.
    ///     Formats can be viewed in a nerdy fashion in the file for <see cref="TimeSpanParser"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ShowTimeFormatInHelpAttribute : Attribute { }
}