﻿using System.Collections.ObjectModel;
using WinHue3.Philips_Hue.BridgeObject;
using WinHue3.Philips_Hue.HueObjects.Common;

namespace WinHue3.Functions.BridgeManager
{
    public sealed partial class BridgesManager
    {

        public ObservableCollection<Bridge> ListBridges
        {
            get => _listBridges;
            private set => SetProperty(ref _listBridges, value);
        }

        public IHueObject SelectedObject
        {
            get => _selectedObject;
            set => SetProperty(ref _selectedObject, value);
        }

        public ObservableCollection<IHueObject> CurrentBridgeHueObjectsList
        {
            get => _listCurrentBridgeHueObjects;
            set => SetProperty(ref _listCurrentBridgeHueObjects, value);
        }

        public Bridge SelectedBridge
        {
            get => _selectedBridge;
            set => SetProperty(ref _selectedBridge, value);
        }

        public bool SearchingLights
        {
            get => _findlighttimer.IsEnabled;
            set => SetProperty(ref _searchingLights, value);
        }

        public bool SearchingSensors
        {
            get => _findsensortimer.IsEnabled;
            set => SetProperty(ref _searchingSensors, value);
        }
    }
}
