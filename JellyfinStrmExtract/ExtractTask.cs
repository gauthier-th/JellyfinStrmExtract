using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace JellyfinStrmExtract
{
    /// <summary>
    /// Task to extract information.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ExtractTask"/> class.
    /// </remarks>
    /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
    /// <param name="providerManager">Instance of the <see cref="IProviderManager"/> interface.</param>
    /// <param name="loggerFactory">Instance of the <see cref="ILoggerFactory"/> interface.</param>
    /// <param name="fileSystem">Instance of the <see cref="IFileSystem"/> interface.</param>
    public class ExtractTask(
        ILibraryManager libraryManager,
        IProviderManager providerManager,
        ILoggerFactory loggerFactory,
        IFileSystem fileSystem) : IScheduledTask
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<ExtractTask>();
        private readonly ILibraryManager _libraryManager = libraryManager;
        private readonly IProviderManager _providerManager = providerManager;
        private readonly IFileSystem _fileSystem = fileSystem;

        /// <inheritdoc/>
        public string Category => "JellyfinStrmExtract";

        /// <inheritdoc/>
        public string Key => "JellyfinStrmExtractTask";

        /// <inheritdoc/>
        public string Description => "Run Strm Media Info Extraction";

        /// <inheritdoc/>
        public string Name => "Process Strm targets";

        /// <inheritdoc/>
        public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
        {
            _logger.LogInformation("JellyfinStrmExtract - Task Execute");

            InternalItemsQuery query = new InternalItemsQuery
            {
                // ExcludeItemTypes = [
                //     BaseItemKind.Folder,
                //     BaseItemKind.CollectionFolder,
                //     BaseItemKind.UserView,
                //     BaseItemKind.Series,
                //     BaseItemKind.Season,
                //     BaseItemKind.Trailer,
                //     BaseItemKind.Playlist
                // ]
            };

            List<BaseItem> results = _libraryManager.GetItemList(query);
            _logger.LogInformation("JellyfinStrmExtract - Number of items before : {Length}", results.Count);
            List<BaseItem> items = [];
            foreach (BaseItem item in results)
            {
                if (!string.IsNullOrEmpty(item.Path) &&
                    item.Path.EndsWith(".strm", StringComparison.InvariantCultureIgnoreCase) &&
                    item.GetMediaStreams().Count == 0)
                {
                    items.Add(item);
                }
                else
                {
                    _logger.LogInformation("JellyfinStrmExtract - Item dropped : {Name} - {Path} - {Type} - {Count}",  item.Name, item.Path, item.GetType(), item.GetMediaStreams().Count);
                }
            }

            _logger.LogInformation("JellyfinStrmExtract - Number of items after : {Count}", items.Count);

            double total = items.Count;
            int current = 0;
            foreach (BaseItem item in items)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("JellyfinStrmExtract - Task Cancelled");
                    break;
                }

                double percent_done = current / total * 100;
                progress.Report(percent_done);

                MetadataRefreshOptions refreshOptions = new MetadataRefreshOptions(new DirectoryService(_fileSystem))
                {
                    EnableRemoteContentProbe = true,
                    ReplaceAllMetadata = true,
                    ImageRefreshMode = MetadataRefreshMode.ValidationOnly,
                    MetadataRefreshMode = MetadataRefreshMode.ValidationOnly,
                    ReplaceAllImages = false
                };

                // ItemUpdateType resp = await item.RefreshMetadata(refreshOptions, cancellationToken);
                await _providerManager.RefreshSingleItem(
                    item,
                    refreshOptions,
                    cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("JellyfinStrmExtract - {Current}/{Total} - {ItemPath}", current, total, item.Path);

                // Thread.Sleep(5000);
                current++;
            }

            progress.Report(100.0);
            _logger.LogInformation("JellyfinStrmExtract - Task Complete");
        }

        /// <inheritdoc/>
        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            return
                [
                    new TaskTriggerInfo
                    {
                        Type = TaskTriggerInfo.TriggerDaily,
                        TimeOfDayTicks = TimeSpan.FromHours(3).Ticks,
                        MaxRuntimeTicks = TimeSpan.FromHours(24).Ticks
                    }
                ];
        }
    }
}
