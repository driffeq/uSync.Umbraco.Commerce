#if NET8_0_OR_GREATER
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;

using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Strings;
using Umbraco.Commerce.Common.Events;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Events.Notification;
using Umbraco.Commerce.Core.Models;

using uSync.BackOffice.Configuration;
using uSync.BackOffice.Services;
using uSync.BackOffice.SyncHandlers;
using uSync.Core;

namespace uSync.Umbraco.Commerce.Handlers;

[SyncHandler("CommerceLocationHandler", "Locations", "Commerce\\Location", CommerceConstants.Priorites.Location,
       Icon = "icon-map-location", EntityType = CommerceConstants.UdiEntityType.Location)]
public class CommerceLocationHandler : CommerceSyncHandlerBase<LocationReadOnly>, ISyncHandler,
    IEventHandlerFor<LocationSavedNotification>,
    IEventHandlerFor<LocationDeletedNotification>
{
    public CommerceLocationHandler(
        ICommerceApi CommerceApi,
        ILogger<CommerceSyncHandlerBase<LocationReadOnly>> logger,
        AppCaches appCaches,
        IShortStringHelper shortStringHelper,
        SyncFileService syncFileService,
        uSyncEventService mutexService,
        uSyncConfigService uSyncConfig,
        ISyncItemFactory itemFactory) 
        : base(CommerceApi, logger, appCaches, shortStringHelper, syncFileService, mutexService, uSyncConfig, itemFactory)
    {  }

    protected override Guid GetStoreId(LocationReadOnly item)
        => item.StoreId;

    protected override IEnumerable<LocationReadOnly> GetByStore(Guid storeId)
        => _CommerceApi.GetLocations(storeId);

    protected override string GetItemName(LocationReadOnly item)
        => item.Name;

    protected override LocationReadOnly GetFromService(Guid key)
        => _CommerceApi.GetLocation(key);

    protected override void DeleteViaService(LocationReadOnly item)
        => _CommerceApi.DeleteLocation(item.Id);

    public override void Handle(SavedNotification<LocationReadOnly> notification)
    {
        foreach(var location in notification.SavedEntities)
        {
            CommerceItemSaved(location);
        }
    }

    public override void Handle(DeletedNotification<LocationReadOnly> notification)
    {
        foreach (var location in notification.DeletedEntities)
        {
            CommerceItemDeleted(location);
        }
    }
}
#endif