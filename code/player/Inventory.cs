using System;
using System.Collections.Generic;
using System.Linq;

using Sandbox;

using TTT.Items;

namespace TTT.Player
{
    public partial class Inventory : BaseInventory
    {
        public readonly PerksInventory Perks;
        public readonly int[] SlotCapacity = new int[] { 1, 1, 1, 3, 3, 1 };

        private const int DROPPOSITIONOFFSET = 50;
        private const int DROPVELOCITY = 500;

        public Inventory(TTTPlayer player) : base(player)
        {
            Perks = new(player);
        }

        public override void DeleteContents()
        {
            foreach (Entity entity in List)
            {
                if (entity is IItem item)
                {
                    item.Remove();
                }
            }

            RPCs.ClientClearInventory(To.Multiple(Utils.GetClients((pl) => pl.CurrentPlayer == Owner as TTTPlayer)));

            Perks.Clear();
            (Owner as TTTPlayer).Ammo.Clear();

            base.DeleteContents();
        }

        public override bool Add(Entity entity, bool makeActive = false)
        {
            TTTPlayer player = Owner as TTTPlayer;

            if (entity is ICarriableItem carriable)
            {
                if (IsCarryingType(entity.GetType()) || !HasEmptySlot(carriable.SlotType))
                {
                    return false;
                }

                RPCs.ClientOnPlayerCarriableItemPickup(To.Multiple(Utils.GetClients((pl) => pl.CurrentPlayer == player)), entity);
                Sound.FromWorld("dm.pickup_weapon", entity.Position);
            }

            return base.Add(entity, makeActive);
        }

        public bool Add(TTTPerk perk)
        {
            return Perks.Give(perk);
        }

        public bool Add(IItem item, bool makeActive = false)
        {
            if (item is Entity ent)
            {
                return Add(ent, makeActive);
            }
            else if (item is TTTPerk perk)
            {
                return Add(perk);
            }

            return false;
        }

        /// <summary>
        /// Tries to add an `TTT.Items.IItem` to the inventory.
        /// </summary>
        /// <param name="item">`TTT.Items.IItem` that will be added to the inventory if conditions are met.</param>
        /// <param name="deleteIfFails">Delete `TTT.Items.IItem` if it fails to add to inventory.</param>
        /// <param name="makeActive">Make `TTT.Items.IItem` the active item in the inventory.</param>
        /// <returns></returns>
        public bool TryAdd(IItem item, bool deleteIfFails = false, bool makeActive = false)
        {
            if (Owner.LifeState != LifeState.Alive || !Add(item, makeActive))
            {
                if (deleteIfFails)
                {
                    item.Delete();
                }

                return false;
            }

            return true;
        }

        public bool Remove(Entity item)
        {
            if (Contains(item))
            {
                RPCs.ClientOnPlayerCarriableItemDrop(To.Single(Owner), item);
                item.Delete();
                List.Remove(item);

                return true;
            }

            return false;
        }

        public bool HasEmptySlot(SlotType slotType)
        {
            int itemsInSlot = List.Count(x => ((ICarriableItem) x).SlotType == slotType);

            return SlotCapacity[(int) slotType - 1] - itemsInSlot > 0;
        }

        public bool IsCarryingType(Type t)
        {
            return List.Any(x => x.GetType() == t);
        }

        public override bool Drop(Entity entity)
        {
            if (!Host.IsServer || !Contains(entity) || entity is ICarriableItem item && !item.CanDrop())
            {
                return false;
            }

            using (Prediction.Off())
            {
                RPCs.ClientOnPlayerCarriableItemDrop(To.Multiple(Utils.GetClients((pl) => pl.CurrentPlayer == Owner as TTTPlayer)), entity);
            }

            return base.Drop(entity);
        }

        public void DropAll()
        {
            List<Entity> cache = new(List);

            foreach (Entity entity in cache)
            {
                Drop(entity);
            }
        }

        public bool DropEntity(Entity self, Type entity)
        {
            Entity droppedEntity = Utils.GetObjectByType<Entity>(entity);
            droppedEntity.Position = Owner.EyePos + Owner.EyeRot.Forward * DROPPOSITIONOFFSET;
            droppedEntity.Rotation = Owner.EyeRot;
            droppedEntity.Velocity = Owner.EyeRot.Forward * DROPVELOCITY;
            droppedEntity.Tags.Add(IItem.ITEM_TAG);

            return Remove(self);
        }
    }
}
