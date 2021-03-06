﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Newtonsoft.Json;
using WinHue3.ExtensionMethods;
using WinHue3.Functions.Application_Settings.Settings;
using WinHue3.Functions.BridgeFinder;
using WinHue3.Functions.BridgePairing;
using WinHue3.Functions.BridgeSettings;
using WinHue3.Interface;
using WinHue3.MainForm;
using WinHue3.Philips_Hue.BridgeObject;
using WinHue3.Philips_Hue.BridgeObject.BridgeObjects;
using WinHue3.Philips_Hue.HueObjects.Common;
using WinHue3.Philips_Hue.HueObjects.GroupObject;
using WinHue3.Philips_Hue.HueObjects.LightObject;
using WinHue3.Philips_Hue.HueObjects.NewSensorsObject;
using WinHue3.Philips_Hue.HueObjects.ResourceLinkObject;
using WinHue3.Philips_Hue.HueObjects.RuleObject;
using WinHue3.Philips_Hue.HueObjects.SceneObject;
using WinHue3.Philips_Hue.HueObjects.ScheduleObject;
using WinHue3.Utils;

namespace WinHue3.Functions.BridgeManager
{
    public sealed partial class BridgesManager
    {
        public void FindSensor()
        {
            _findsensortimer.Start();
            SearchingSensors = _findsensortimer.IsEnabled;
        }

        public void FindLight()
        {           
            _findlighttimer.Start();
            SearchingLights = _findlighttimer.IsEnabled;
        }

        private void RefreshHueObject(ref IHueObject obj, IHueObject newobject)
        {
            if (obj == null || newobject == null) return;
            if (obj.GetType() != newobject.GetType()) return;
            if (obj.Id != newobject.Id) return;

            PropertyInfo[] pi = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (PropertyInfo p in pi)
            {
                object value = p.GetValue(newobject);
                p.SetValue(obj, value);
            }
        }

        public async Task ChangeBridge()
        {
            await RefreshCurrentListHueObject();
            _refreshTimer.Start();
            RaisePropertyChanged("UpdateAvailable");
        }

        public void LoadVirtualBridge()
        {
            System.Windows.Forms.OpenFileDialog fd = new System.Windows.Forms.OpenFileDialog
            {
                Filter = @"Text files (*.txt)|*.txt"
            };
            if (fd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            string file = File.ReadAllText(fd.FileName);
            DataStore ds = JsonConvert.DeserializeObject<DataStore>(file);
            Bridge vbridge = new Bridge() { Virtual = true, Name = "Virtual Bridge", RequiredUpdate = false };
            ListBridges.Add(vbridge);
            SelectedBridge = vbridge;
            CurrentBridgeHueObjectsList = new ObservableCollection<IHueObject>(ds.ToList());
            CreateExpanders();

        }

        public async Task RefreshCurrentObject(bool logging = false)
        {
            int index = CurrentBridgeHueObjectsList.FindIndex(x => x.Id == SelectedObject.Id && x.GetType() == SelectedObject.GetType());
            if (index == -1) return;

            IHueObject hr = await SelectedBridge.GetObjectAsync(SelectedObject.Id, SelectedObject.GetType());

            if (hr == null) return;
            IHueObject newobj = hr;
            CurrentBridgeHueObjectsList[index].Image = newobj.Image;
            List<PropertyInfo> pi = newobj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly).ToList();

            foreach (PropertyInfo p in pi)
            {
                if (CurrentBridgeHueObjectsList[index].HasProperty(p.Name))
                {
                    PropertyInfo prop = CurrentBridgeHueObjectsList[index].GetType().GetProperty(p.Name);
                    if (prop != null) p.SetValue(CurrentBridgeHueObjectsList[index], prop.GetValue(newobj));
                }
            }
        }

        public async Task ChangeBridgeSettings()
        {
            if (SelectedBridge == null) return;
            Form_BridgeSettings frs = new Form_BridgeSettings { Owner = Application.Current.MainWindow };
            await frs.Initialize(SelectedBridge);
            if (frs.ShowDialog() == true)
            {
                Philips_Hue.BridgeObject.BridgeObjects.BridgeSettings bresult = await SelectedBridge.GetBridgeSettingsAsyncTask();
                if (bresult != null)
                {
                    string newname = bresult.name;
                    if (SelectedBridge.Name != newname)
                    {
                        SelectedBridge.Name = newname;
                        ListBridges[ListBridges.IndexOf(SelectedBridge)].Name = newname;
                        Bridge selbr = SelectedBridge;
                        SelectedBridge = null;
                        SelectedBridge = selbr;
                    }

                    await RefreshCurrentListHueObject();
                    RaisePropertyChanged("UpdateAvailable");
                }
            }
        }

        public async Task RefreshCurrentListHueObject()
        {
            if (SelectedBridge == null) return;
            Cursor_Tools.ShowWaitCursor();
            SelectedObject = null;
            if (!SelectedBridge.Virtual)
            {
                log.Info($"Getting list of objects from bridge at {SelectedBridge.IpAddress}.");
                List<IHueObject> hr = await SelectedBridge.GetAllObjectsAsync(false, true);
                if (hr != null)
                {
                    List<IHueObject> listobj = hr;

                    ObservableCollection<IHueObject> newlist = new ObservableCollection<IHueObject>();

                    switch (WinHueSettings.settings.Sort)
                    {
                        case WinHueSortOrder.Default:
                            CurrentBridgeHueObjectsList = new ObservableCollection<IHueObject>(hr);
                            break;
                        case WinHueSortOrder.Ascending:
                            newlist.AddRange(from item in listobj
                                             where item is Light
                                             orderby item.name
                                             select item);
                            newlist.AddRange(from item in listobj
                                             where item is Group
                                             orderby item.name
                                             select item);
                            newlist.AddRange(from item in listobj
                                             where item is Schedule
                                             orderby item.name
                                             select item);
                            newlist.AddRange(from item in listobj
                                             where item is Scene
                                             orderby item.name
                                             select item);
                            newlist.AddRange(
                                from item in listobj where item is Sensor orderby item.name select item);
                            newlist.AddRange(from item in listobj where item is Rule orderby item.name select item);
                            newlist.AddRange(from item in listobj
                                             where item is Resourcelink
                                             orderby item.name
                                             select item);
                            CurrentBridgeHueObjectsList = new ObservableCollection<IHueObject>(newlist);
                            break;
                        case WinHueSortOrder.Descending:
                            newlist.AddRange(from item in listobj
                                             where item is Light
                                             orderby item.name descending
                                             select item);
                            newlist.AddRange(from item in listobj
                                             where item is Group
                                             orderby item.name descending
                                             select item);
                            newlist.AddRange(from item in listobj
                                             where item is Schedule
                                             orderby item.name descending
                                             select item);
                            newlist.AddRange(from item in listobj
                                             where item is Scene
                                             orderby item.name descending
                                             select item);
                            newlist.AddRange(from item in listobj
                                             where item is Sensor
                                             orderby item.name descending
                                             select item);
                            newlist.AddRange(from item in listobj
                                             where item is Rule
                                             orderby item.name descending
                                             select item);
                            newlist.AddRange(from item in listobj
                                             where item is Resourcelink
                                             orderby item.name descending
                                             select item);
                            CurrentBridgeHueObjectsList = new ObservableCollection<IHueObject>(newlist);
                            break;
                        default:
                            goto case WinHueSortOrder.Default;
                    }

                    CreateExpanders();

                }
                else
                {
                    CurrentBridgeHueObjectsList = new ObservableCollection<IHueObject>();
                    log.Info("Bridge update required. Please update the bridge to use WinHue with this bridge.");
                }
            }
            else
            {
                log.Info("Virtual Bridge detected. Will skip refresh.");
            }

            Cursor_Tools.ShowNormalCursor();
        }

        private void CreateExpanders()
        {
            log.Info($"Found {CurrentBridgeHueObjectsList.Count} objects in the bridge.");
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(CurrentBridgeHueObjectsList);
            view.GroupDescriptions?.Clear();
            PropertyGroupDescription groupDesc = new TypeGroupDescription();
            view.GroupDescriptions?.Add(groupDesc);
            log.Info($"Finished refreshing view.");
        }

        public void AddBridge(Bridge bridge)
        {
            ListBridges.Add(bridge);
            OnBridgeAdded?.Invoke(bridge);
        }

        public void RemoveBridge(Bridge bridge)
        {
            ListBridges.Remove(bridge);
            OnBridgeRemoved?.Invoke(bridge);
        }

        public bool DoBridgePairing()
        {
            Form_BridgeDetectionPairing dp = new Form_BridgeDetectionPairing(new ObservableCollection<Bridge>(ListBridges)) { Owner = Application.Current.MainWindow };
            bool result = dp.ShowDialog().GetValueOrDefault(false);
            if (!result) return result;
            ListBridges = dp.ViewModel.ListBridges;

            SaveSettings();
            return result;
        }

        private bool CheckBridge(Bridge bridge)
        {
            log.Info("Checking if ip is bridge...");
            BasicConfig bc = bridge.GetBridgeBasicConfig();
            if (bc == null) return false;
            bridge.ApiVersion = bc.apiversion;
            bridge.Name = bc.name;
            bridge.SwVersion = bc.swversion;
            WinHueSettings.bridges.BridgeInfo[bridge.Mac].name = bridge.Name;
            WinHueSettings.SaveBridges();
            return true;

        }

        private bool SaveSettings()
        {
            foreach (Bridge br in _listBridges)
            {
                if (br.Mac == string.Empty) continue;
                if (WinHueSettings.bridges.BridgeInfo.ContainsKey(br.Mac))
                    WinHueSettings.bridges.BridgeInfo[br.Mac] = new BridgeSaveSettings
                    {
                        ip = br.IpAddress.ToString(),
                        apikey = br.ApiKey,
                        name = br.Name,
                    };
                else
                    WinHueSettings.bridges.BridgeInfo.Add(br.Mac,
                        new BridgeSaveSettings { ip = br.IpAddress.ToString(), apikey = br.ApiKey, name = br.Name });

                if (br.IsDefault) WinHueSettings.bridges.DefaultBridge = br.Mac;
            }

            return WinHueSettings.SaveBridges();
        }

        public void LoadBridges()
        {
            while (true)
            {
                log.Info("Loading bridge(s)...");

                log.Info($"Checking if any bridge already present in settings... found {WinHueSettings.bridges.BridgeInfo.Count}");

                if (WinHueSettings.bridges.BridgeInfo.Count == 0 || WinHueSettings.bridges.DefaultBridge == string.Empty || !WinHueSettings.bridges.BridgeInfo.ContainsKey(WinHueSettings.bridges.DefaultBridge))
                {   // No bridge found in the list of bridge.
                    log.Info("Either no bridge found in settings or no default bridge. Pairing needed.");
                    if (DoBridgePairing())
                        continue;
                    else
                        break;
                }

                foreach (KeyValuePair<string, BridgeSaveSettings> b in WinHueSettings.bridges.BridgeInfo)
                {
                    log.Info($"Bridge OK. Checking if bridge already in the bridge list...");
                    if (ListBridges.All(x => x.Mac != b.Key))
                    {
                        Bridge bridge = new Bridge()
                        {
                            ApiKey = b.Value.apikey,
                            IpAddress = IPAddress.Parse(b.Value.ip),
                            Name = b.Value.name,
                            IsDefault = b.Key == WinHueSettings.bridges.DefaultBridge,
                            Mac = b.Key
                        };
                        if (b.Value.apikey == string.Empty) continue;

                        bridge.LastCommandMessages.OnMessageAdded += LastCommandMessages_OnMessageAdded;
                        bridge.RequiredUpdate = WinHueSettings.settings.CheckForBridgeUpdate && UpdateManager.CheckBridgeNeedUpdate(bridge.ApiVersion);

                        log.Info("Bridge not in the list adding it...");
                        ListBridges.Add(bridge);

                    }
                    else
                    {
                        log.Info("Bridge already in the list skipping...");
                    }
                }

                foreach (Bridge br in ListBridges)
                {
                    log.Info($"Checking bridge {br}");
                    if (!CheckBridge(br))
                    {
                        log.Info("Bridge IP has changed... Pairing needed.");
                        Form_BridgeFinder fbf = new Form_BridgeFinder(br) { Owner = Application.Current.MainWindow };
                        fbf.ShowDialog();

                        if (fbf.IpFound())
                        {
                            br.BridgeNotResponding += Br_BridgeNotResponding;
                            br.IpAddress = fbf.newip;
                            if (!br.IsDefault) continue;
                            SelectedBridge = br;
                        }
                        else
                        {
                            DoBridgePairing();
                            break;
                        }
                    }
                    else
                    {
                        if (!br.IsDefault) continue;
                        SelectedBridge = br;
                    }
                }


                break;
            }
            
        }
    }
}
