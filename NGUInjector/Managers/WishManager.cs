using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using static NGUInjector.Main;

namespace NGUInjector.Managers
{
    internal class WishManager
    {
        private readonly Character _character;
        private readonly List<int> _curValidUpgradesList = new List<int>();

        public WishManager()
        {
            _character = Main.Character;
        }

        public int GetSlot(int slotId)
        {
            BuildWishList();
            if (slotId + 1 > _curValidUpgradesList.Count)
            {
                return -1;
            }
            return _curValidUpgradesList[slotId];
        }

        public void BuildWishList()
        {
            var wishList = new List<Tuple<int, bool, double, int>>();

            _curValidUpgradesList.Clear();

            for (var i = 0; i < _character.wishes.wishes.Count; i++)
            {
                if (!IsValidWish(i))
                {
                    continue;
                }

                bool isOnPriorityList = Settings.WishPriorities.Contains(i);
                double sortValue = -1;
                if (!isOnPriorityList || Settings.WishSortPriorities)
                {
                    sortValue = GetSortValue(i);
                }
                int tieBreaker = isOnPriorityList ? Array.IndexOf(Settings.WishPriorities, i) : i;

                wishList.Add(new Tuple<int, bool, double, int>(i, isOnPriorityList, sortValue, tieBreaker));
            }

            //foreach (var wish in wishList.OrderByDescending(x => x.Item2).ThenBy(x => x.Item3).ThenBy(x => x.Item4))
            //{
            //    LogDebug($"Wish {wish.Item1}: Prioritized:{wish.Item2} | SortValue:{wish.Item3} | TieBreaker:{wish.Item4}");
            //}

            _curValidUpgradesList.AddRange(wishList.OrderByDescending(x => x.Item2).ThenBy(x => x.Item3).ThenBy(x => x.Item4).Select(x => x.Item1));





            //var dictDouble = new Dictionary<int, double>();

            //_curValidUpgradesList.Clear();

            //for (var i = 0; i < Settings.WishPriorities.Length; i++)
            //{
            //    if (IsValidWish(Settings.WishPriorities[i]))
            //    {
            //        if (Settings.WishSortPriorities)
            //        {
            //            dictDouble.Add(Settings.WishPriorities[i], GetSortValue(Settings.WishPriorities[i]) + i);
            //        }
            //        else
            //        {
            //            //LogDebug($"Wish {Settings.WishPriorities[i]}: Prioritized:{true} | SortValue:{-1}");
            //            _curValidUpgradesList.Add(Settings.WishPriorities[i]);
            //        }
            //    }
            //}
            //if (Settings.WishSortPriorities)
            //{
            //    dictDouble = (from x in dictDouble
            //                  orderby x.Value
            //                  select x).ToDictionary(x => x.Key, x => x.Value);
            //    for (var j = 0; j < dictDouble.Count; j++)
            //    {
            //        //LogDebug($"Wish {dictDouble.ElementAt(j).Key}: Prioritized:{true} | SortValue:{dictDouble.ElementAt(j).Value}");
            //        _curValidUpgradesList.Add(dictDouble.ElementAt(j).Key);
            //    }
            //    dictDouble = new Dictionary<int, double>();
            //}
            //for (var i = 0; i < _character.wishes.wishes.Count; i++)
            //{
            //    if (_curValidUpgradesList.Contains(i))
            //    {
            //        continue;
            //    }
            //    if (IsValidWish(i))
            //    {
            //        dictDouble.Add(i, this.GetSortValue(i) + i);
            //    }
            //}
            //dictDouble = (from x in dictDouble
            //              orderby x.Value
            //              select x).ToDictionary(x => x.Key, x => x.Value);
            //for (var j = 0; j < dictDouble.Count; j++)
            //{
            //    //LogDebug($"Wish {dictDouble.ElementAt(j).Key}: Prioritized:{false} | SortValue:{dictDouble.ElementAt(j).Value}");
            //    _curValidUpgradesList.Add(dictDouble.ElementAt(j).Key);
            //}
        }

        public bool IsValidWish(int wishId)
        {
            if (wishId < 0 || wishId > _character.wishes.wishSize())
            {
                return false;
            }
            if (_character.wishesController.properties[wishId].difficultyRequirement > _character.wishesController.character.settings.rebirthDifficulty)
            {
                return false;
            }
            if (_character.wishesController.progressPerTickMax(wishId) <= 0f)
            {
                return false;
            }
            if (_character.wishesController.character.wishes.wishes[wishId].level >= _character.wishesController.properties[wishId].maxLevel)
            {
                return false;
            }
            if (Settings.WishBlacklist.Length > 0 && Settings.WishBlacklist.Contains(wishId))
            {
                return false;
            }
            return true;
        }

        public double GetSortValue(int wishId)
        {
            if (Settings.WishSortOrder)
            {
                return _character.wishesController.wishSpeedDivider(wishId) * (1f - _character.wishes.wishes[wishId].progress);
            }
            return _character.wishesController.properties[wishId].wishSpeedDivider;
        }
    }
}