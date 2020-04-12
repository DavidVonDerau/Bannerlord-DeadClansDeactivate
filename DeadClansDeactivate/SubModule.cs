using System.Linq;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.EncyclopediaItems;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia;

namespace DeadClansDeactivate
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            var harmony = new Harmony($"mod.bannerlord.dvd.{nameof(DeadClansDeactivate)}");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(EncyclopediaFactionPageVM), nameof(EncyclopediaFactionPageVM.Refresh))]
    class EncyclopediaFactionPageVMPatch
    {
        // ReSharper disable once InconsistentNaming
        private static readonly FieldInfo _faction;

        static EncyclopediaFactionPageVMPatch()
        {
            _faction = typeof(EncyclopediaFactionVM).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Single(fieldInfo => fieldInfo.Name == nameof(_faction));
        }

        // ReSharper disable once InconsistentNaming
        static void Postfix(EncyclopediaFactionPageVM __instance)
        {
            __instance.IsLoadingOver = false;
            for (var idx = 0; idx < __instance.Clans.Count; ++idx)
            {
                var original = __instance.Clans[idx];
                __instance.Clans[idx] = new EncyclopediaFactionImprovedVM((IFaction)_faction.GetValue(original));
            }
            __instance.IsLoadingOver = true;
        }
    }

    public sealed class EncyclopediaFactionImprovedVM : EncyclopediaFactionVM
    {
        [DataSourceProperty]
        public bool IsClanDestroyed
        {
            get => _isClanDestroyed;
            set
            {
                if (value == _isClanDestroyed)
                    return;
                _isClanDestroyed = value;
                OnPropertyChanged(nameof(IsClanDestroyed));
            }
        }

        private bool _isClanDestroyed;

        private readonly IFaction _faction;

        public EncyclopediaFactionImprovedVM(IFaction faction)
            : base(faction)
        {
            _faction = faction;
            RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            if (_faction == null)
            {
                return;
            }

            IsClanDestroyed = _faction.IsDeactivated;
        }
    }
}
