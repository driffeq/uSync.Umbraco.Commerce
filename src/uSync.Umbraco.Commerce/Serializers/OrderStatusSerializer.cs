﻿using Microsoft.Extensions.Logging;
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

namespace uSync.Umbraco.Commerce.Serializers
{
    [SyncSerializer("FA15B3E1-8100-431E-BC95-4B74134A42DD", "OrderStatus Serializer", CommerceConstants.Serialization.OrderStatus)]
    public class OrderStatusSerializer : CommerceSerializerBase<OrderStatusReadOnly>, ISyncSerializer<OrderStatusReadOnly>
    {
        public OrderStatusSerializer(ICommerceApi CommerceApi, CommerceSyncSettingsAccessor settingsAccessor,
            IUnitOfWorkProvider uowProvider,
            ILogger<OrderStatusSerializer> logger) : base(CommerceApi, settingsAccessor, uowProvider, logger)
        { }

        protected override SyncAttempt<XElement> SerializeCore(OrderStatusReadOnly item, SyncSerializerOptions options)
        {
            var node = InitializeBaseNode(item, ItemAlias(item));

            node.Add(new XElement(nameof(item.Name), item.Name));
            node.Add(new XElement(nameof(item.SortOrder), item.SortOrder));
            node.Add(new XElement(nameof(item.Color), item.Color));
            node.AddStoreId(item.StoreId);

            return SyncAttemptSucceedIf(node != null, item.Name, node, ChangeType.Export);
        }

        protected override SyncAttempt<OrderStatusReadOnly> DeserializeCore(XElement node, SyncSerializerOptions options)
        {
            var readonlyItem = FindItem(node);

            var alias = node.GetAlias();
            var id = node.GetKey();
            var name = node.Element(nameof(readonlyItem.Name)).ValueOrDefault(alias);
            var storeId = node.GetStoreId();

            using (var uow = _uowProvider.Create())
            {
                OrderStatus item;
                if (readonlyItem == null)
                {
                    item = OrderStatus.Create(uow, id, storeId, alias, name);
                }
                else
                {
                    item = readonlyItem.AsWritable(uow);
                    item.SetAlias(alias)
                        .SetName(name);
                }

                item.SetColor(node.Element(nameof(item.Color)).ValueOrDefault(item.Color));
                item.SetSortOrder(node.Element(nameof(item.SortOrder)).ValueOrDefault(item.SortOrder));

                _CommerceApi.SaveOrderStatus(item);
                uow.Complete();

                return SyncAttemptSucceed(name, item.AsReadOnly(), ChangeType.Import);
            }
        }

        public override bool IsValid(XElement node)
            => base.IsValid(node)
            && node.GetStoreId() != Guid.Empty;

        public override string GetItemAlias(OrderStatusReadOnly item)
            => item.Alias;

        public override void DoDeleteItem(OrderStatusReadOnly item)
            => _CommerceApi.DeleteOrderStatus(item.Id);

        public override OrderStatusReadOnly DoFindItem(Guid key)
            => _CommerceApi.GetOrderStatus(key);

        public override void DoSaveItem(OrderStatusReadOnly item)
        {
            using (var uow = _uowProvider.Create())
            {
                var entity = item.AsWritable(uow);
                _CommerceApi.SaveOrderStatus(entity);
                uow.Complete();
            }
        }
    }
}
