using System;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Serialization;

namespace JellyfinStrmExtract;

/// <summary>
/// The main plugin.
/// </summary>
public class Plugin : BasePlugin<PluginConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
    /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    /// <inheritdoc />
    public override string Name => "JellyfinStrmExtract";

    /// <inheritdoc />
    public override Guid Id => Guid.Parse("5b9adc74-c34c-41bb-b8da-bbccd9e72455");

    /// <summary>
    /// Gets the current plugin instance.
    /// </summary>
    public static Plugin? Instance { get; private set; }
}
