﻿using System;
using System.Linq;
using Huoyaoyuan.AdmiralRoom.API;
using Huoyaoyuan.AdmiralRoom.Notifier;
using Meowtrix.ComponentModel;
using Newtonsoft.Json.Linq;

namespace Huoyaoyuan.AdmiralRoom.Officer.Battle
{
    public class BattleManager : NotificationObject
    {
        public BattleManager()
        {
            Staff.API("api_port/port").Subscribe((Fiddler.Session x) =>
            {
                InSortie = false;
                CurrentMap = null;
                CurrentNode = null;
                if (SortieFleet1 != null)
                {
                    SortieFleet1.InSortie = false;
                    foreach (var ship in SortieFleet1.Ships) ship.IsEscaped = false;
                }
                if (SortieFleet2 != null)
                {
                    SortieFleet2.InSortie = false;
                    foreach (var ship in SortieFleet2.Ships) ship.IsEscaped = false;
                }
                SortieFleet1 = null;
                SortieFleet2 = null;
                CurrentBattle = null;
                GetShipEquip = null;
                CurrentFleetType = null;
                lastescapeinfo = null;
            });
            Staff.API("api_req_map/next").Subscribe<map_start_next>(StartNextHandler);
            Staff.API("api_req_sortie/battleresult").Subscribe<sortie_battleresult>(BattleResultHandler);
            Staff.API("api_req_combined_battle/battleresult").Subscribe<sortie_battleresult>(BattleResultHandler);
            Staff.API("api_req_map/start").Subscribe<map_start_next>((req, api) =>
            {
                InSortie = true;
                SortieFleet1 = Staff.Current.Homeport.Fleets[req.GetInt("api_deck_id")];
                SortieFleet1.InSortie = true;
                CurrentFleetType = Staff.Current.Homeport.CombinedFleet;
                if (SortieFleet1.Id == 1 && Staff.Current.Homeport.CombinedFleet != CombinedFleetType.None)
                {
                    SortieFleet2 = Staff.Current.Homeport.Fleets[2];
                    SortieFleet2.InSortie = true;
                }
                Logger.Loggers.MaterialLogger.ForceLog = true;
                StartNextHandler(api);
            });
            Staff.API("api_req_sortie/battle").Subscribe<sortie_battle>(StartBattle);
            Staff.API("api_req_battle_midnight/battle").Subscribe<sortie_battle>(NightBattle);
            Staff.API("api_req_battle_midnight/sp_midnight").Subscribe<sortie_battle>(StartBattle);
            Staff.API("api_req_practice/battle").Subscribe<sortie_battle>(StartBattle);
            Staff.API("api_req_practice/midnight_battle").Subscribe<sortie_battle>(NightBattle);
            Staff.API("api_req_sortie/airbattle").Subscribe<sortie_battle>(StartBattle);
            Staff.API("api_req_sortie/ld_airbattle").Subscribe<sortie_battle>(StartBattle);
            Staff.API("api_req_sortie/goback_port").Subscribe(EscapeHandler);
            Staff.API("api_req_combined_battle/airbattle").Subscribe<sortie_battle>(StartBattle);
            Staff.API("api_req_combined_battle/battle").Subscribe<sortie_battle>(StartBattle);
            Staff.API("api_req_combined_battle/midnight_battle").Subscribe<sortie_battle>(NightBattle);
            Staff.API("api_req_combined_battle/sp_midnight").Subscribe<sortie_battle>(StartBattle);
            Staff.API("api_req_combined_battle/battle_water").Subscribe<sortie_battle>(StartBattle);
            Staff.API("api_req_combined_battle/ld_airbattle").Subscribe<sortie_battle>(StartBattle);
            Staff.API("api_req_combined_battle/goback_port").Subscribe(EscapeHandler);
            Staff.API("api_req_combined_battle/ec_battle").Subscribe<sortie_battle>(StartBattle);
            Staff.API("api_req_combined_battle/ec_midnight_battle").Subscribe<sortie_battle>(NightBattle);
            Staff.API("api_req_combined_battle/each_battle").Subscribe<sortie_battle>(StartBattle);
            Staff.API("api_req_combined_battle/each_battle_water").Subscribe<sortie_battle>(StartBattle);
            Staff.API("api_req_combined_battle/ec_night_to_day").Subscribe<sortie_battle>(StartBattle);
        }
        public Fleet SortieFleet1 { get; private set; }
        public Fleet SortieFleet2 { get; private set; }
        private Ship FindShip(int index) => index <= SortieFleet1.Ships.Count ? SortieFleet1.Ships[index - 1] : SortieFleet2.Ships[index - 7];

        #region InSortie
        private bool _insortie;
        public bool InSortie
        {
            get { return _insortie; }
            set
            {
                if (_insortie != value)
                {
                    _insortie = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region CurrentMap
        private MapInfo _currentmap;
        public MapInfo CurrentMap
        {
            get { return _currentmap; }
            set
            {
                if (_currentmap != value)
                {
                    _currentmap = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region CurrentNode
        private MapNode _currentnode;
        public MapNode CurrentNode
        {
            get { return _currentnode; }
            set
            {
                if (_currentnode != value)
                {
                    _currentnode = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region CurrentBattle
        private BattleBase _currentbattle;
        public BattleBase CurrentBattle
        {
            get { return _currentbattle; }
            set
            {
                _currentbattle = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region CurrentFleetType
        private CombinedFleetType? _currentfleettype;
        public CombinedFleetType? CurrentFleetType
        {
            get { return _currentfleettype; }
            set
            {
                if (_currentfleettype != value)
                {
                    _currentfleettype = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        private int? GetShipEquip;
        private sortie_battleresult.escape lastescapeinfo;
        private void StartBattle(sortie_battle api)
        {
            ShipInBattle[] fleet1, fleet2;
            fleet1 = (SortieFleet1?.Ships ?? Staff.Current.Homeport.Fleets[api.api_deck_id].Ships)
                .Select((x, i) => new ShipInBattle(x) { Index = i + 1 }).ToArray();
            fleet2 = SortieFleet2?.Ships
                .Select((x, i) => new ShipInBattle(x) { Index = i + 7 }).ToArray();
            if (SortieFleet1 == null)//演习
                Staff.Current.Homeport.Fleets[api.api_deck_id].Ships.ForEach(x => x.IgnoreNextCondition());
            CurrentBattle = new Battle(api, CurrentFleetType ?? CombinedFleetType.None, CurrentNode?.Type ?? MapNodeType.Battle, fleet1, fleet2);
        }
        private void NightBattle(sortie_battle api) =>
            (CurrentBattle as Battle).NightBattle(api);
        private void StartNextHandler(map_start_next api)
        {
            CurrentMap = Staff.Current.MasterData.MapAreas[api.api_maparea_id][api.api_mapinfo_no];
            CurrentNode = new MapNode(api);
            SortieFleet1?.Ships.ForEach(y => y.IgnoreNextCondition());
            SortieFleet2?.Ships.ForEach(y => y.IgnoreNextCondition());
            CurrentBattle = new BattleBase(this);
            if (GetShipEquip != null)
            {
                Staff.Current.Admiral.ShipCount++;
                Staff.Current.Admiral.EquipCount += GetShipEquip.Value;
            }
            GetShipEquip = null;

            var heavydamage = SortieFleet1.Ships.Skip(1).ConcatNotNull(SortieFleet2?.Ships.Skip(1))
                .Where(x => !x.IsEscaped && x.HP.Current * 4 <= x.HP.Max);
            if (heavydamage.Any())
                NotifierFactories.Current.Show(StringTable.Notification_HeavyDamage_Title,
                    heavydamage.Aggregate(StringTable.Notification_HeavyDamage_Text, (text, ship) => text += "\n" + ship));
        }
        private void BattleResultHandler(sortie_battleresult api)
        {
            SortieFleet1?.Ships.ForEach(y => y.IgnoreNextCondition());
            SortieFleet2?.Ships.ForEach(y => y.IgnoreNextCondition());
            if (CurrentNode.Type.IsBOSS())
            {
                StaticCounters.BossCounter.Increase();
                if (ConstData.RanksWin.Contains(api.api_win_rank))
                {
                    StaticCounters.BossWinCounter.Increase();
                    if (CurrentMap.AreaNo == 2) StaticCounters.Map2Counter.Increase();
                    else if (CurrentMap.AreaNo == 3 && CurrentMap.No >= 3) StaticCounters.Map3Counter.Increase();
                    else if (CurrentMap.AreaNo == 4) StaticCounters.Map4Counter.Increase();
                    else if (CurrentMap.Id == 15) StaticCounters.Map1_5Counter.Increase();
                }
            }
            lastescapeinfo = api.api_escape;
            if (!Enum.TryParse(api.api_win_rank, out WinRank winrank))
                winrank = (CurrentBattle as Battle).WinRank;
            else if (winrank == WinRank.S && (CurrentBattle as Battle)?.FriendDamageRate == 0)
                winrank = WinRank.Perfect;
            CurrentBattle.GetShip = api.api_get_ship?.api_ship_name ?? StringTable.Empty;
            GetShipEquip = api.api_get_ship == null ? null : (int?)0;//TODO:记录船附带的装备
            var now = DateTime.UtcNow;
            Logger.Loggers.BattleDropLogger.Log(new Logger.BattleDropLog
            {
                DateTime = now,
                MapArea = CurrentMap.Id,
                MapCell = CurrentNode.Id,
                IsBOSS = CurrentNode.Type.IsBOSS(),
                MapAreaName = CurrentMap.Name,
                EnemyFleetName = api.api_enemy_info?.api_deck_name ?? "",
                WinRank = winrank,
                DropShipId = api.api_get_ship?.api_ship_id ??
                    (Staff.Current.Admiral.CanDropShip ? 0 : -1),
                DropItem = api.api_get_useitem?.api_useitem_id ?? 0
            });
            Logger.Loggers.BattleDetailLogger.SetTimeStamp(now);
            if (Staff.Current.Admiral.CanDropShip)
                Reporter.PoiDBReporter.ReportAsync(new JObject
                {
                    ["mapId"] = CurrentMap.Id,
                    ["cellId"] = CurrentNode.Id,
                    ["isBoss"] = CurrentNode.Type.IsBOSS(),
                    ["shipId"] = api.api_get_ship?.api_ship_id ?? -1,
                    ["enemy"] = api.api_enemy_info.api_deck_name,
                    ["quest"] = api.api_quest_name,
                    ["mapLv"] = (int)CurrentMap.Difficulty,
                    ["rank"] = api.api_win_rank,
                    ["teitokuLv"] = api.api_member_lv,
                    ["enemyShips"] = new JArray(CurrentBattle.EnemyShipIds),
                    ["enemyFormation"] = (int)(CurrentBattle as Battle).EnemyFormation
                }, "drop_ship");
            //if (api.api_get_eventitem != null)
            //{
            //    Reporter.PoiDBReporter.ReportAsync(new JObject
            //    {
            //        ["teitokuId"] = Staff.Current.Admiral.MemberID,
            //        ["teitokuLv"] = Staff.Current.Admiral.Level,
            //        ["teitoku"] = Staff.Current.Admiral.Nickname,
            //        ["mapId"] = CurrentMap.Id,
            //        ["mapLv"] = (int)CurrentMap.Difficulty
            //    }, "pass_event");
            //}
            foreach (var enemy in CurrentBattle.AllEnemies)
                if (enemy.ToHP <= 0)
                    switch (enemy.ShipInfo.ShipType.Id)
                    {
                        case 13://潜水艦
                            StaticCounters.SSCounter.Increase();
                            break;
                        case 15://補給艦
                            StaticCounters.TransportCounter.Increase();
                            break;
                        case 7://軽空母
                        case 11://正規空母
                            StaticCounters.CVCounter.Increase();
                            break;
                    }
            Staff.Current.Quests.Save();
        }
        private void EscapeHandler(CachedSession x)
        {
            if (lastescapeinfo != null)
            {
                FindShip(lastescapeinfo.api_escape_idx[0]).IsEscaped = true;
                if (lastescapeinfo.api_tow_idx != null)
                    FindShip(lastescapeinfo.api_tow_idx[0]).IsEscaped = true;
            }
        }
    }
}
