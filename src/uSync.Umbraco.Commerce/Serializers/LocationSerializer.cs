#if NET8_0_OR_GREATER
using Microsoft.Extensions.Logging;

using System;
using System.Xml.Linq;

using Umbraco.Commerce.Common;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;

using uSync.Core;
using uSync.Core.Models;
using uSync.Core.Serialization;
using uSync.Umbraco.Commerce.Configuration;
using uSync.Umbraco.Commerce.Extensions;

namespace uSync.Umbraco.Commerce.Serializers;

[SyncSerializer("26892D7B-6C23-4EAA-8902-91730E4C86BB", "Location Serializer", CommerceConstants.Serialization.Location)]
public class CommerceLocationSerializer :
    MethodSerializerBase<LocationReadOnly>, ISyncSerializer<LocationReadOnly>
{
    public CommerceLocationSerializer(
        ICommerceApi CommerceApi,
        CommerceSyncSettingsAccessor settingsAccessor,
        IUnitOfWorkProvider uowProvider,
        ILogger<MethodSerializerBase<LocationReadOnly>> logger) : base(CommerceApi, settingsAccessor, uowProvider, logger)
    { }

    protected override SyncAttempt<XElement> SerializeCore(LocationReadOnly item, SyncSerializerOptions options)
    {
        var node = InitializeBaseNode(item, ItemAlias(item));

        node.Add(new XElement(nameof(item.Name), item.Name));
        node.Add(new XElement(nameof(item.Type), item.Type));
        node.Add(new XElement(nameof(item.SortOrder), item.SortOrder));

        node.Add(new XElement(nameof(item.AddressLine1), item.AddressLine1));
        node.Add(new XElement(nameof(item.AddressLine2), item.AddressLine2));
        node.Add(new XElement(nameof(item.City), item.City));
        node.Add(new XElement(nameof(item.ZipCode), item.ZipCode));
        node.Add(new XElement(nameof(item.CountryIsoCode), item.CountryIsoCode));
        node.AddStoreId(item.StoreId);

        return SyncAttemptSucceedIf(node != null, item.Alias, node, Core.ChangeType.Export);
    }

    protected override SyncAttempt<LocationReadOnly> DeserializeCore(XElement node, SyncSerializerOptions options)
    {
        var readonlyItem = FindItem(node);

        var alias = node.GetAlias();
        var key = node.GetKey();
        var name = node.Element(nameof(readonlyItem.Name)).ValueOrDefault(alias);
        var storeId = node.GetStoreId();

        using (var uow = _uowProvider.Create())
        {
            Location location;
            if (readonlyItem is null)
            {
                location = Location.Create(uow, storeId, alias, name);
            }
            else
            {
                location = readonlyItem.AsWritable(uow);
                location
                    .SetAlias(alias)
                    .SetName(name);
            }

            location.SetType(node.Element(nameof(location.Type)).ValueOrDefault(location.Type));
            location.SetSortOrder(node.Element(nameof(location.SortOrder)).ValueOrDefault(location.SortOrder));

            var address = new Address(
                addressLine1: node.Element(nameof(location.AddressLine1)).ValueOrDefault(location.AddressLine1),
                addressLine2: node.Element(nameof(location.AddressLine2)).ValueOrDefault(location.AddressLine2),
                city: node.Element(nameof(location.City)).ValueOrDefault(location.City),
                region: node.Element(nameof(location.Region)).ValueOrDefault(location.Region),
                countryIsoCode: node.Element(nameof(location.CountryIsoCode)).ValueOrDefault(location.CountryIsoCode),
                zipCode: node.Element(nameof(location.ZipCode)).ValueOrDefault(location.ZipCode));

            location.SetAddress(address);

            _CommerceApi.SaveLocation(location);

            uow.Complete();

            return SyncAttemptSucceed(name, location.AsReadOnly(), ChangeType.Import);

        }


    }


    public override string GetItemAlias(LocationReadOnly item)
        => item.Alias;

    public override void DoDeleteItem(LocationReadOnly item)
        => _CommerceApi.DeleteLocation(item.Id);

    public override LocationReadOnly DoFindItem(Guid key)
        => _CommerceApi.GetLocation(key);

    public override void DoSaveItem(LocationReadOnly item)
    {
        using (var uow = _uowProvider.Create())
        {
            var entity = item.AsWritable(uow);
            _CommerceApi.SaveLocation(entity);
            uow.Complete();
        }
    }
}
#endif