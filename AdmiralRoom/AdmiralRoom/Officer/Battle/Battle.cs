﻿using System.Linq;
using Huoyaoyuan.AdmiralRoom.API;
using Meowtrix.Linq;
using static System.Math;

namespace Huoyaoyuan.AdmiralRoom.Officer.Battle
{
    public class Battle : BattleBase
    {
        public override bool IsBattling => true;
        public MapNodeType BattleType { get; set; }
        private ShipInBattle[] NightOrTorpedo => Fleet2 ?? Fleet1;
        public Formation FriendFormation { get; set; }
        public Formation EnemyFormation { get; set; }
        public Direction Direction { get; set; }
        public int FriendSearching { get; set; }
        public int EnemySearching { get; set; }
        public int AnonymousFriendDamage { get; set; }
        public int AnonymousEnemyDamage { get; set; }
        public ShipInBattle[] FriendFleet { get; private set; }
        public double FriendDamageRate => (double)AllFriends.Sum(x => x.FromHP - x.ToHP)
            / Fleet1.ConcatNotNull(Fleet2).Sum(x => x.FromHP);
        public double EnemyDamageRate => (double)AllEnemies.Sum(x => x.FromHP - x.ToHP) / AllEnemies.Sum(x => x.FromHP);
        public int FriendLostCount => AllFriends.Count(x => x.ToHP <= 0);
        public int EnemySinkCount => AllEnemies.Count(x => x.ToHP <= 0);
        public WinRank WinRank
        {
            get
            {
                int fl = FriendLostCount;
                int es = EnemySinkCount;
                int fd = (int)(FriendDamageRate * 100);
                int ed = (int)(EnemyDamageRate * 100);
                int ec = AllEnemies.Count();
                if (BattleType == MapNodeType.AirDefence)//空袭战
                {
                    if (FriendDamageRate <= 0) return WinRank.Perfect;
                    if (fd <= 10) return WinRank.A;
                    if (fd <= 20) return WinRank.B;
                    if (fd <= 50) return WinRank.C;
                    return WinRank.D;
                }
                if (fl == 0)
                {
                    if (es == ec)
                    {
                        return FriendDamageRate <= 0 ? WinRank.Perfect : WinRank.S;
                    }
                    if (es >= Round(ec * 0.625)) return WinRank.A;
                    if (EnemyFleet[0].ToHP <= 0) return WinRank.B;
                    if (ed > fd * 2.5) return WinRank.B;
                    if (ed > fd * 0.9) return WinRank.C;
                    return WinRank.D;
                }
                else
                {
                    if (es == ec) return WinRank.B;
                    if (EnemyFleet[0].ToHP <= 0 && fl < es) return WinRank.B;
                    if (ed > fd * 2.5) return WinRank.B;
                    if (ed > fd * 0.9) return WinRank.C;
                    if (fl >= Round(Fleet1.ConcatNotNull(Fleet2).Count() * 0.6)) return WinRank.E;
                    return WinRank.D;
                }
            }
        }
        public ShipInBattle FindFriend(int index)
            => index < Fleet1.Length ? Fleet1[index] : Fleet2[index - 6];
        public ShipInBattle FindEnemy(int index)
            => index < EnemyFleet.Length ? EnemyFleet[index] : EnemyFleet2[index - 6];

        #region Stages
        public JetPlaneAttack Jet { get; }
        public JetPlaneAttack AirBaseJet { get; }
        public AirBaseAttack[] AirBaseAttacks { get; }
        public AerialCombat AirCombat1 { get; }
        public AerialCombat AirCombat2 { get; }
        public SupportAttack Support { get; private set; }
        public FireCombat OpeningASW { get; }
        public TorpedoCombat OpeningTorpedo { get; }
        public FireCombat FireStage1 { get; }
        public FireCombat FireStage2 { get; }
        public FireCombat FireStage3 { get; }
        public TorpedoCombat TorpedoStage { get; }
        public NightCombat FriendNight { get; private set; }
        public NightCombat Night { get; private set; }
        public FireCombat NightToDay1 { get; }
        public FireCombat NightToDay2 { get; }
        #endregion

        public Battle(sortie_battle api, CombinedFleetType fleettype, MapNodeType battletype, ShipInBattle[] fleet1, ShipInBattle[] fleet2)
        {
            FleetType = fleettype;
            BattleType = battletype;
            Fleet1 = fleet1;
            Fleet2 = fleet2;

            bool isEnemyCombined = battletype == MapNodeType.Combined || battletype == MapNodeType.CombinedBOSS;

            if (api.api_formation != null)
            {
                FriendFormation = (Formation)api.api_formation[0];
                EnemyFormation = (Formation)api.api_formation[1];
                Direction = (Direction)api.api_formation[2];
            }
            if (api.api_search != null)
            {
                FriendSearching = api.api_search[0];
                EnemySearching = api.api_search[1];
            }

            EnemyFleet = api.api_ship_ke
                .Select((x, i) => new ShipInBattle
                {
                    Index = i + 1,
                    IsEnemy = true,
                    ShipInfo = Staff.Current.MasterData.ShipInfo[x],
                    Level = api.api_ship_lv[i],
                    Equipments = api.api_eSlot[i].Select(y => Staff.Current.MasterData.EquipInfo[y]).Where(y => y != null).Select(y => new EquipInBattle(y)).ToArray(),
                    Firepower = api.api_eParam[i][0],
                    Torpedo = api.api_eParam[i][1],
                    AA = api.api_eParam[i][2],
                    Armor = api.api_eParam[i][3]
                })
                .ToArray();
            EnemyFleet2 = api.api_ship_ke_combined?
                .Select((x, i) => new ShipInBattle
                {
                    Index = i + 7,
                    IsEnemy = true,
                    ShipInfo = Staff.Current.MasterData.ShipInfo[x],
                    Level = api.api_ship_lv_combined[i],
                    Equipments = api.api_eSlot_combined[i].Select(y => Staff.Current.MasterData.EquipInfo[y]).Where(y => y != null).Select(y => new EquipInBattle(y)).ToArray(),
                    Firepower = api.api_eParam_combined[i][0],
                    Torpedo = api.api_eParam_combined[i][1],
                    AA = api.api_eParam_combined[i][2],
                    Armor = api.api_eParam_combined[i][3]
                })
                .ToArray();

            EnemyShipIds = api.api_ship_ke.ConcatNotNull(api.api_ship_ke_combined).ToArray();

            SetHPs(Fleet1, api.api_f_nowhps, api.api_f_maxhps);
            SetHPs(EnemyFleet, api.api_e_nowhps, api.api_e_maxhps);
            SetHPs(Fleet2, api.api_f_nowhps_combined, api.api_f_maxhps_combined);
            SetHPs(EnemyFleet2, api.api_e_nowhps_combined, api.api_e_maxhps_combined);

            api.api_escape_idx?.ForEach(x => Fleet1[x - 1].IsEscaped = true);
            api.api_escape_idx_combined?.ForEach(x => Fleet2[x - 1].IsEscaped = true);

            if (api.api_n_support_info != null)
                Support = new SupportAttack(this, api.api_n_support_info, api.api_n_support_flag);
            if (api.api_n_hougeki1 != null)
                NightToDay1 = new FireCombat(this, api.api_n_hougeki1);
            if (api.api_n_hougeki2 != null)
                NightToDay2 = new FireCombat(this, api.api_n_hougeki2);
            if (api.api_air_base_injection != null)
                AirBaseJet = new JetPlaneAttack(this, api.api_air_base_injection, true);
            if (api.api_injection_kouku != null)
                Jet = new JetPlaneAttack(this, api.api_injection_kouku, false);
            if (api.api_air_base_attack != null)
                AirBaseAttacks = api.api_air_base_attack.Select(x => new AirBaseAttack(this, x)).ToArray();
            if (api.api_kouku != null)
                AirCombat1 = new AerialCombat(this, api.api_kouku);
            if (api.api_kouku2 != null)
                AirCombat2 = new AerialCombat(this, api.api_kouku2);
            if (api.api_support_flag != 0)
                Support = new SupportAttack(this, api.api_support_info, api.api_support_flag);
            if (api.api_opening_taisen != null)
                OpeningASW = new FireCombat(this, api.api_opening_taisen);
            if (api.api_opening_atack != null)
                OpeningTorpedo = new TorpedoCombat(this, api.api_opening_atack);
            if (isEnemyCombined)
                switch (fleettype)
                {
                    case CombinedFleetType.None:
                        if (api.api_hougeki1 != null)
                            FireStage1 = new FireCombat(this, api.api_hougeki1);
                        if (api.api_raigeki != null)
                            TorpedoStage = new TorpedoCombat(this, api.api_raigeki);
                        if (api.api_hougeki2 != null)
                            FireStage2 = new FireCombat(this, api.api_hougeki2);
                        if (api.api_hougeki3 != null)
                            FireStage3 = new FireCombat(this, api.api_hougeki3);
                        break;
                    case CombinedFleetType.Carrier:
                    case CombinedFleetType.Transport:
                        if (api.api_hougeki1 != null)
                            FireStage1 = new FireCombat(this, api.api_hougeki1);
                        if (api.api_hougeki2 != null)
                            FireStage2 = new FireCombat(this, api.api_hougeki2);
                        if (api.api_raigeki != null)
                            TorpedoStage = new TorpedoCombat(this, api.api_raigeki);
                        if (api.api_hougeki3 != null)
                            FireStage3 = new FireCombat(this, api.api_hougeki3);
                        break;
                    case CombinedFleetType.Surface:
                        if (api.api_hougeki1 != null)
                            FireStage1 = new FireCombat(this, api.api_hougeki1);
                        if (api.api_hougeki2 != null)
                            FireStage2 = new FireCombat(this, api.api_hougeki2);
                        if (api.api_hougeki3 != null)
                            FireStage3 = new FireCombat(this, api.api_hougeki3);
                        if (api.api_raigeki != null)
                            TorpedoStage = new TorpedoCombat(this, api.api_raigeki);
                        break;
                }
            else
                switch (fleettype)
                {
                    case CombinedFleetType.None:
                        if (api.api_hougeki1 != null)
                            FireStage1 = new FireCombat(this, api.api_hougeki1);
                        if (api.api_hougeki2 != null)
                            FireStage2 = new FireCombat(this, api.api_hougeki2);
                        if (api.api_raigeki != null)
                            TorpedoStage = new TorpedoCombat(this, api.api_raigeki);
                        break;
                    case CombinedFleetType.Carrier:
                    case CombinedFleetType.Transport:
                        if (api.api_hougeki1 != null)
                            FireStage1 = new FireCombat(this, api.api_hougeki1);
                        if (api.api_raigeki != null)
                            TorpedoStage = new TorpedoCombat(this, api.api_raigeki);
                        if (api.api_hougeki2 != null)
                            FireStage2 = new FireCombat(this, api.api_hougeki2);
                        if (api.api_hougeki3 != null)
                            FireStage3 = new FireCombat(this, api.api_hougeki3);
                        break;
                    case CombinedFleetType.Surface:
                        if (api.api_hougeki1 != null)
                            FireStage1 = new FireCombat(this, api.api_hougeki1);
                        if (api.api_hougeki2 != null)
                            FireStage2 = new FireCombat(this, api.api_hougeki2);
                        if (api.api_hougeki3 != null)
                            FireStage3 = new FireCombat(this, api.api_hougeki3);
                        if (api.api_raigeki != null)
                            TorpedoStage = new TorpedoCombat(this, api.api_raigeki);
                        break;
                }
            if (api.api_hougeki != null || api.api_friendly_info != null)
                NightBattle(api);
            else EndApplyBattle();
        }
        private static void SetHPs(ShipInBattle[] fleet, int[] hps, int[] maxhps)
        {
            if (fleet == null) return;
            for (int i = 0; i < fleet.Length; i++)
            {
                var ship = fleet[i];
                ship.MaxHP = maxhps[i];
                ship.FromHP = ship.ToHP = hps[i];
            }
        }

        public void NightBattle(sortie_battle api)
        {
            if (api.api_friendly_info != null)
            {
                var friend = api.api_friendly_info;
                FriendFleet = friend.api_ship_id
                    .Select((x, i) => new ShipInBattle
                    {
                        Index = i + 1,
                        ShipInfo = Staff.Current.MasterData.ShipInfo[x],
                        Level = friend.api_ship_lv[i],
                        Equipments = friend.api_Slot[i].Select(y => Staff.Current.MasterData.EquipInfo[y]).Where(y => y != null).Select(y => new EquipInBattle(y)).ToArray(),
                        Firepower = friend.api_Param[i][0],
                        Torpedo = friend.api_Param[i][1],
                        AA = friend.api_Param[i][2],
                        Armor = friend.api_Param[i][3]
                    }).ToArray();
                SetHPs(FriendFleet, friend.api_nowhps, friend.api_maxhps);
                var fleet1 = Fleet1;
                Fleet1 = FriendFleet;
                FriendNight = new NightCombat(this, api.api_friendly_battle, FriendFleet, AllEnemies.ToArray());
                Fleet1 = fleet1;
            }

            if (api.api_active_deck != null)
                Night = new NightCombat(this, api, NightOrTorpedo, api.api_active_deck[1] == 1 ? EnemyFleet : EnemyFleet2);
            else Night = new NightCombat(this, api, NightOrTorpedo, EnemyFleet);
            EndApplyBattle();
        }
        private void EndApplyBattle()
        {
            void OnEndUpdate(ShipInBattle ship)
            {
                ship.EndUpdate();
                ship.IsMostDamage = false;
            }
            Fleet1.ForEach(OnEndUpdate);
            Fleet2?.ForEach(OnEndUpdate);
            EnemyFleet.ForEach(OnEndUpdate);
            EnemyFleet2?.ForEach(OnEndUpdate);
            //mvp
            Fleet1.TakeMaxOrDefault(x => x.DamageGiven).SetMvp();
            Fleet2?.TakeMaxOrDefault(x => x.DamageGiven).SetMvp();
            EnemyFleet.TakeMaxOrDefault(x => x.DamageGiven).SetMvp();
            EnemyFleet2?.TakeMaxOrDefault(x => x.DamageGiven).SetMvp();

            OnAllPropertyChanged();
        }
    }
    public enum Formation { 単縦陣 = 1, 複縦陣 = 2, 輪形陣 = 3, 梯形陣 = 4, 単横陣 = 5, 警戒陣 = 6, 第一警戒航行序列 = 11, 第二警戒航行序列 = 12, 第三警戒航行序列 = 13, 第四警戒航行序列 = 14 }
    public enum Direction { 同航戦 = 1, 反航戦 = 2, T字有利 = 3, T字不利 = 4 }
    public enum WinRank { Perfect, S, A, B, C, D, E }
    public enum AirControl { 制空互角 = 0, 制空権確保 = 1, 航空優勢 = 2, 航空劣勢 = 3, 制空権喪失 = 4 }
}
