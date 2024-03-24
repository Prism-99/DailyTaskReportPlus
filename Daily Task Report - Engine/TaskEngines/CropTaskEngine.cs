﻿using System.Text;
using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;
using StardewValley;
using DailyTasksReport.Tasks;
using DailyTasksReport.UI;
using StardewValley.TokenizableStrings;

namespace DailyTasksReport.TaskEngines
{
    class CropTaskEngine : TaskEngine
    {
        public readonly CropsTaskId _id;
        private readonly int _index;
        private readonly string _locationName;
        private bool _anyCrop;
        //
        //  added for 1.6
        //
        private static Dictionary<string, List<Tuple<Vector2, HoeDirt>>> newCrops = new();
        private static Dictionary<string, List<Tuple<Vector2, FruitTree>>> newFruitTrees = new();


        // 0 = Farm, 1 = Greenhouse, 2 = IslandWest
        private static readonly List<Tuple<Vector2, HoeDirt>>[] Crops =
            {new List<Tuple<Vector2, HoeDirt>>(), new List<Tuple<Vector2, HoeDirt>>(), new List<Tuple<Vector2, HoeDirt>>()};

        private static readonly List<Tuple<Vector2, FruitTree>>[] FruitTrees =
            {new List<Tuple<Vector2, FruitTree>>(), new List<Tuple<Vector2, FruitTree>>(), new List<Tuple<Vector2, FruitTree>>()};

        public static CropsTaskId _who = CropsTaskId.None;


        internal CropTaskEngine(TaskReportConfig config, CropsTaskId id)
        {
            _config = config;
            _id = id;
            TaskClass = "Crop";
            TaskSubClass = id.ToString();
            TaskId = (int)id;

            switch (id)
            {
                case CropsTaskId.UnwateredCropFarm:
                case CropsTaskId.UnharvestedCropFarm:
                case CropsTaskId.DeadCropFarm:
                case CropsTaskId.FruitTreesFarm:
                    _index = 0;
                    _locationName = "Farm";
                    break;
                case CropsTaskId.UnwateredCropWestIsland:
                case CropsTaskId.UnharvestedCropWestIsland:
                case CropsTaskId.DeadCropWestIsland:
                case CropsTaskId.FruitTreesWestIsland:
                    _index = 2;
                    _locationName = "IslandWest";
                    break;
                default:
                    _index = 1;
                    _locationName = "Greenhouse";
                    break;
            }
            SetEnabled();
        }


        public override void Clear()
        {
            if (_who == _id)
            {
                Crops[0].Clear();
                Crops[1].Clear();
                Crops[2].Clear();
                FruitTrees[0].Clear();
                FruitTrees[1].Clear();
                FruitTrees[2].Clear();
                newCrops.Clear();
                newFruitTrees.Clear();
            }
            SetEnabled();
        }
        public override void SetEnabled()
        {
            switch (_id)
            {
                case CropsTaskId.UnwateredCropFarm:
                case CropsTaskId.UnwateredCropGreenhouse:
                case CropsTaskId.UnwateredCropWestIsland:
                    Enabled = _config.UnwateredCrops;
                    break;

                case CropsTaskId.UnharvestedCropFarm:
                case CropsTaskId.UnharvestedCropGreenhouse:
                case CropsTaskId.UnharvestedCropWestIsland:
                    Enabled = _config.UnharvestedCrops;
                    break;

                case CropsTaskId.DeadCropFarm:
                case CropsTaskId.DeadCropGreenhouse:
                case CropsTaskId.DeadCropWestIsland:
                    Enabled = _config.DeadCrops;
                    break;

                case CropsTaskId.FruitTreesFarm:
                case CropsTaskId.FruitTreesGreenhouse:
                case CropsTaskId.FruitTreesWestIsland:
                    Enabled = _config.FruitTrees > 0;
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"Crop task or location not implemented");
            }

        }

        public override List<ReportReturnItem> DetailedInfo()
        {
            List<ReportReturnItem> rpItem = new List<ReportReturnItem> { };

            if (!Enabled || !_anyCrop) return rpItem;

            var stringBuilder = new StringBuilder();

            switch (_id)
            {
                case CropsTaskId.UnwateredCropWestIsland:
                case CropsTaskId.UnwateredCropFarm:
                    stringBuilder.Append(I18n.Tasks_Crop_Label_Unwatered());

                    break;

                case CropsTaskId.UnharvestedCropFarm:
                case CropsTaskId.UnharvestedCropWestIsland:
                    stringBuilder.Append(I18n.Tasks_Crop_Label_Harvest());
                    break;

                case CropsTaskId.DeadCropFarm:
                case CropsTaskId.DeadCropWestIsland:
                    stringBuilder.Append(I18n.Tasks_Crop_Label_Crops());
                    break;

                case CropsTaskId.FruitTreesFarm:
                case CropsTaskId.FruitTreesWestIsland:
                    stringBuilder.Append(I18n.Tasks_Crop_Label_Fruit());
                    break;

                default:
                    break;
            }

            switch (_id)
            {
                case CropsTaskId.UnwateredCropFarm:
                case CropsTaskId.UnwateredCropGreenhouse:
                case CropsTaskId.UnwateredCropWestIsland:
                    rpItem.AddRange(EchoForCrops(pair => pair.Item2.state.Value == HoeDirt.dry && pair.Item2.needsWatering() && !pair.Item2.crop.dead.Value));
                    break;

                case CropsTaskId.UnharvestedCropFarm:
                case CropsTaskId.UnharvestedCropGreenhouse:
                case CropsTaskId.UnharvestedCropWestIsland:
                    rpItem.AddRange(EchoForCrops(pair => pair.Item2.readyForHarvest()));
                    break;

                case CropsTaskId.DeadCropFarm:
                case CropsTaskId.DeadCropGreenhouse:
                case CropsTaskId.DeadCropWestIsland:
                    rpItem.AddRange(EchoForCrops(pair => pair.Item2.crop.dead.Value));
                    break;

                case CropsTaskId.FruitTreesFarm:
                case CropsTaskId.FruitTreesGreenhouse:
                case CropsTaskId.FruitTreesWestIsland:
                    if (_config.FruitTrees == 0) break;

                    if (_index == 0)
                    {
                        foreach(string location in newFruitTrees.Keys)
                        {
                            foreach (var tuple in newFruitTrees[location].Where(t => t.Item2.fruit.Count >= _config.FruitTrees))
                            {
                                string sFruits = (tuple.Item2.fruit.Count > 1) ? I18n.Tasks_Crop_WithFruit() : I18n.Tasks_Crop_WithFruits();
                                rpItem.Add(new ReportReturnItem
                                {

                                    Label = ObjectsNames[tuple.Item2.fruit[0].ParentSheetIndex.ToString()] + I18n.Tasks_Crop_TreeAt() + GetLocationDisplayName(location) + I18n.Tasks_Crop_With() + tuple.Item2.fruit.Count.ToString() + sFruits + " (" + tuple.Item1.X.ToString() + ", " + tuple.Item1.Y.ToString() + ")",
                                    WarpTo = new Tuple<string, int, int>(location, (int)tuple.Item1.X, (int)tuple.Item1.Y)
                                });
                            }
                        }
                    }
                    else
                    {
                        foreach (var tuple in FruitTrees[_index].Where(t => t.Item2.fruit.Count >= _config.FruitTrees))
                        {
                            string sFruits = (tuple.Item2.fruit.Count > 1) ? I18n.Tasks_Crop_WithFruit() : I18n.Tasks_Crop_WithFruits();
                            rpItem.Add(new ReportReturnItem
                            {

                                Label = ObjectsNames[tuple.Item2.fruit[0].ParentSheetIndex.ToString()] + I18n.Tasks_Crop_TreeAt() + _locationName + I18n.Tasks_Crop_With() + tuple.Item2.fruit.Count.ToString() + sFruits + " (" + tuple.Item1.X.ToString() + ", " + tuple.Item1.Y.ToString() + ")",
                                WarpTo = new Tuple<string, int, int>(_locationName, (int)tuple.Item1.X, (int)tuple.Item1.Y)
                            });
                        }
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"Crop task or location not implemented");
            }

            return rpItem;
        }

        public override List<ReportReturnItem> GeneralInfo()
        {
            List<ReportReturnItem> prItem = new List<ReportReturnItem> { };

            if (!Enabled) return prItem;

            _anyCrop = false;
            int count;
            if (_index == 0)
            {
                if (newCrops.Count == 0)
                {
                    var locations = Game1.locations.Where(p => p.Name != "Greenhouse" && p.Name != "IslandWest" && p.terrainFeatures.Where(p => p.Values.Where(o => o is HoeDirt && ((HoeDirt)o).crop != null && !((HoeDirt)o).crop.forageCrop).Any()).Any());
                    foreach (var cropLocation in locations)
                    {
                        newCrops.Add(cropLocation.Name, new List<Tuple<Vector2, HoeDirt>> { });
                        newCrops[cropLocation.Name].AddRange(cropLocation.terrainFeatures.Pairs.Where(p => p.Value is HoeDirt && ((HoeDirt)p.Value).crop != null).Select(o => Tuple.Create(o.Key, o.Value as HoeDirt)));
                    }
                }
                if (newFruitTrees.Count == 0)
                    LoadFruitTreeData();
            }
            else
            {
                if (Crops[_index].Count == 0)
                {
                    var location = Game1.locations.FirstOrDefault(l => l.Name == _locationName);
                    foreach (var keyValuePair in location.terrainFeatures.Pairs)
                        if (keyValuePair.Value is HoeDirt dirt && dirt.crop != null)
                            Crops[_index].Add(new Tuple<Vector2, HoeDirt>(keyValuePair.Key, dirt));
                }
            }
            switch (_id)
            {
                case CropsTaskId.UnwateredCropFarm:
                case CropsTaskId.UnwateredCropGreenhouse:
                case CropsTaskId.UnwateredCropWestIsland:
                    if (_index == 0)
                    {
                        foreach (string location in newCrops.Keys)
                        {
                            count = newCrops[location].Count(pair =>
                                 pair.Item2.state.Value == HoeDirt.dry && pair.Item2.needsWatering() && !pair.Item2.crop.dead.Value);
                            if (count > 0)
                            {
                                _anyCrop = true;
                                prItem.Add(new ReportReturnItem { Label = GetLocationDisplayName(location) + " " + I18n.Tasks_Crop_NotWatered(), Details = count.ToString() });
                            }
                        }
                    }
                    else
                    {
                        count = Crops[_index].Count(pair =>
                        pair.Item2.state.Value == HoeDirt.dry && pair.Item2.needsWatering() && !pair.Item2.crop.dead.Value);
                        if (count > 0)
                        {
                            _anyCrop = true;
                            prItem.Add(new ReportReturnItem { Label = _locationName + " " + I18n.Tasks_Crop_NotWatered(), Details = count.ToString() });
                        }
                    }
                    break;

                case CropsTaskId.UnharvestedCropFarm:
                case CropsTaskId.UnharvestedCropGreenhouse:
                case CropsTaskId.UnharvestedCropWestIsland:
                    count = 0;
                    if (_index == 0)
                    {
                        foreach (string location in newCrops.Keys)
                        {
                            foreach (Tuple<Vector2, HoeDirt> item in newCrops[location])
                            {
                                if (item.Item2.readyForHarvest())
                                {
                                    if (_config.SkipFlowersInHarvest && (_config.FlowerReportLastDay && Game1.dayOfMonth != 28))
                                    {
                                        if (ObjectsNames.ContainsKey(item.Item2.crop.indexOfHarvest.Value.ToString()))
                                        {
                                            int cropCategory = Game1.objectData[item.Item2.crop.indexOfHarvest.Value].Category;

                                            if (cropCategory != -80)
                                            {
                                                count++;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        count++;
                                    }
                                }
                            }

                            if (count > 0)
                            {
                                _anyCrop = true;
                                prItem.Add(new ReportReturnItem { Label = GetLocationDisplayName(location) + " " + I18n.Tasks_Crop_ReadyToHarvest(), Details = count.ToString() });
                            }
                        }
                    }
                    else
                    {
                        foreach (Tuple<Vector2, HoeDirt> item in Crops[_index])
                        {
                            if (item.Item2.readyForHarvest())
                            {
                                if (_config.SkipFlowersInHarvest && (_config.FlowerReportLastDay && Game1.dayOfMonth != 28))
                                {
                                    if (ObjectsNames.ContainsKey(item.Item2.crop.indexOfHarvest.Value.ToString()))
                                    {

                                        string sCat = Game1.objectData[item.Item2.crop.indexOfHarvest.Value].Category.ToString();

                                        string[] arSubCat = sCat.Split(' ');

                                        if (arSubCat.Length == 1 || arSubCat[1] != "-80")
                                        {
                                            count++;
                                        }
                                    }
                                }
                                else
                                {
                                    count++;
                                }
                            }
                        }

                        if (count > 0)
                        {
                            _anyCrop = true;
                            prItem.Add(new ReportReturnItem { Label = _locationName + " " + I18n.Tasks_Crop_ReadyToHarvest(), Details = count.ToString() });
                        }
                    }
                    break;

                case CropsTaskId.DeadCropFarm:
                case CropsTaskId.DeadCropGreenhouse:
                case CropsTaskId.DeadCropWestIsland:
                    if (_index == 0)
                    {
                        foreach (string location in newCrops.Keys)
                        {
                            count = newCrops[location].Count(pair => pair.Item2.crop.dead.Value);
                            if (count > 0)
                            {
                                _anyCrop = true;
                                prItem.Add(new ReportReturnItem { Label = I18n.Tasks_Crop_Dead() + " " + GetLocationDisplayName(location), Details = count.ToString() });
                            }
                        }
                    }
                    else
                    {
                        count = Crops[_index].Count(pair => pair.Item2.crop.dead.Value);
                        if (count > 0)
                        {
                            _anyCrop = true;
                            prItem.Add(new ReportReturnItem { Label = I18n.Tasks_Crop_Dead() + " " + _locationName, Details = count.ToString() });
                        }
                    }
                    break;

                case CropsTaskId.FruitTreesFarm:
                case CropsTaskId.FruitTreesGreenhouse:
                case CropsTaskId.FruitTreesWestIsland:

                    if (_index == 0)
                    {
                        foreach(string key in newFruitTrees.Keys)
                        {
                            count = newFruitTrees[key].Count(p => p.Item2.fruit.Count >= _config.FruitTrees);

                            if (count > 0)
                            {
                                _anyCrop = true;
                                prItem.Add(new ReportReturnItem { Label = I18n.Tasks_Crop_Fruit() + " " + GetLocationDisplayName(key), Details = count.ToString() });
                            }
                        }
                    }
                    else
                    {
                        count = FruitTrees[_index].Count(p => p.Item2.fruit.Count >= _config.FruitTrees);

                        if (count > 0)
                        {
                            _anyCrop = true;
                            prItem.Add(new ReportReturnItem { Label = I18n.Tasks_Crop_Fruit() + " " + _locationName, Details = count.ToString() });
                        }
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"Crop task or location not implemented");
            }

            return prItem;
        }
        private void LoadFruitTreeData()
        {
            var plantable = DataLoader.Locations(Game1.content).Where(p => (p.Value?.CanPlantHere ?? false) && p.Key != "IslandWest" && p.Key != "Greenhouse");


            foreach (var p in plantable)
            {
                GameLocation plantableLocation = Game1.getLocationFromName(p.Key);

                if (plantableLocation != null)
                {
                    if (!newFruitTrees.ContainsKey(p.Key))
                        newFruitTrees.Add(p.Key, new List<Tuple<Vector2, FruitTree>> { });

                    foreach (var pair in plantableLocation.terrainFeatures.Pairs)

                        if (pair.Value is FruitTree tree && tree.fruit.Count > 0)
                            newFruitTrees[p.Key].Add(new Tuple<Vector2, FruitTree>(pair.Key, tree));
                }
            }

        }
        internal override void FirstScan()
        {
            if (_who == CropsTaskId.None)
                _who = _id;

            if (ObjectsNames.Count == 0)
                PopulateObjectsNames();

            if (_who != _id) return;

            LoadFruitTreeData();
 
            var location = Game1.locations.FirstOrDefault(l => l.IsGreenhouse);
            if (location != null)
            {
                foreach (var pair in location.terrainFeatures.Pairs)

                    if (pair.Value is FruitTree tree && tree.fruit.Count > 0)
                        FruitTrees[1].Add(new Tuple<Vector2, FruitTree>(pair.Key, tree));
            }

            location = Game1.locations.FirstOrDefault(l => l.Name == "IslandWest");
            if (location != null)
            {
                foreach (var pair in location.terrainFeatures.Pairs)

                    if (pair.Value is FruitTree tree && tree.fruit.Count > 0)
                        FruitTrees[2].Add(new Tuple<Vector2, FruitTree>(pair.Key, tree));
            }
        }

        public override void FinishedReport()
        {
            Crops[_index].Clear();
            newCrops.Clear();
            newFruitTrees.Clear();
        }
        private List<ReportReturnItem> EchoForCrops(Func<Tuple<Vector2, HoeDirt>, bool> predicate)
        {
            List<ReportReturnItem> prItem = new List<ReportReturnItem> { };
            if (_index == 0)
            {
                foreach (string location in newCrops.Keys)
                {
                    foreach (var item in newCrops[location].Where(predicate))
                    {
                        if (ObjectsNames.ContainsKey(item.Item2.crop.indexOfHarvest.Value.ToString()))
                        {

                            int categoryId = Game1.objectData[item.Item2.crop.indexOfHarvest.Value].Category;

                            if (!_config.SkipFlowersInHarvest || (_config.SkipFlowersInHarvest && categoryId != -80)
                                || (_config.FlowerReportLastDay && Game1.dayOfMonth == 28))
                            {
                                prItem.Add(new ReportReturnItem
                                {
                                    Label = ObjectsNames[item.Item2.crop.indexOfHarvest.Value.ToString()] + I18n.Tasks_At() + location + " (" + item.Item1.X.ToString() + "," + item.Item1.Y.ToString() + ")",
                                    WarpTo = new Tuple<string, int, int>(location, (int)item.Item1.X, (int)item.Item1.Y)
                                });
                            }
                        }
                        else
                        {
                            prItem.Add(new ReportReturnItem
                            {
                                Label = "Unknown Id:  " + item.Item2.crop.indexOfHarvest.Value + I18n.Tasks_At() + location + " (" + item.Item1.X.ToString() + "," + item.Item1.Y.ToString() + ")",
                                WarpTo = new Tuple<string, int, int>(location, (int)item.Item1.X, (int)item.Item1.Y)
                            }); ;
                        }
                    }

                }
            }
            else
            {
                foreach (var item in Crops[_index].Where(predicate))
                {
                    if (ObjectsNames.ContainsKey(item.Item2.crop.indexOfHarvest.Value.ToString()))
                    {

                        string sCat = Game1.objectData[item.Item2.crop.indexOfHarvest.Value].Category.ToString();

                        string[] arSubCat = sCat.Split(' ');

                        if (!_config.SkipFlowersInHarvest || (_config.SkipFlowersInHarvest && (arSubCat.Length == 1 || arSubCat[1] != "-80"))
                            || (_config.FlowerReportLastDay && Game1.dayOfMonth == 28))
                        {
                            prItem.Add(new ReportReturnItem
                            {
                                Label = ObjectsNames[item.Item2.crop.indexOfHarvest.Value.ToString()] + I18n.Tasks_At() + _locationName + " (" + item.Item1.X.ToString() + "," + item.Item1.Y.ToString() + ")",
                                WarpTo = new Tuple<string, int, int>(_locationName, (int)item.Item1.X, (int)item.Item1.Y)
                            });
                        }
                    }
                    else
                    {
                        prItem.Add(new ReportReturnItem
                        {
                            Label = "Unknown Id:  " + item.Item2.crop.indexOfHarvest.Value + I18n.Tasks_At() + _locationName + " (" + item.Item1.X.ToString() + "," + item.Item1.Y.ToString() + ")",
                            WarpTo = new Tuple<string, int, int>(_locationName, (int)item.Item1.X, (int)item.Item1.Y)
                        }); ;
                    }
                }
            }
            return prItem;
        }
    }
}