﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using log4net.Core;
using WinHue3.ExtensionMethods;
using WinHue3.Philips_Hue.BridgeObject;
using WinHue3.Philips_Hue.BridgeObject.BridgeObjects;
using WinHue3.Philips_Hue.Communication;
using WinHue3.Philips_Hue.HueObjects.Common;
using WinHue3.Philips_Hue.HueObjects.GroupObject;
using WinHue3.Philips_Hue.HueObjects.LightObject;
using WinHue3.Philips_Hue.HueObjects.NewSensorsObject;
using WinHue3.Philips_Hue.HueObjects.SceneObject;
using WinHue3.Philips_Hue.HueObjects.ScheduleObject;
using WinHue3.Utils;
using Group = WinHue3.Philips_Hue.HueObjects.GroupObject.Group;

namespace WinHue3.Functions.Schedules.NewCreator
{
    public enum ContentTypeVm { Light, Sensor, Group, Schedule, Scene };

    public class ScheduleCreatorViewModel : ValidatableBindableBase
    {

        private ObservableCollection<IHueObject> _listTargetHueObject;
        private ValidatableBindableBase _selectedViewModel;
        private Bridge _bridge;
        private ScheduleCreatorHeader _header;
        private ContentTypeVm _content;
        private List<IHueObject> _currentHueObjectList;
        private IHueObject _selectedHueObject;
        private string _effect;
        private string _dateTimeFormat;
        private string _smask;
        private int _repetitions;
        private HueAddress _adrTarget;
        private bool _propGridLG;

        public ICommand SelectTargetObject => new RelayCommand(param => SelectTarget());
        public ICommand ChangeDateTimeFormat => new RelayCommand(param => ChangeDateTime());
        public ICommand UsePropertyGridLGCommand => new RelayCommand(param => UsePropertyGridLG());

        private void UsePropertyGridLG()
        {
            if (Content != ContentTypeVm.Light && Content != ContentTypeVm.Group) return;
            if (_selectedViewModel is ScheduleCreatorSlidersViewModel)
            {
                SelectedViewModel = new ScheduleCreatorPropertyGridViewModel();
            }
            else
            {
                SelectedViewModel = new ScheduleCreatorSlidersViewModel();
            }
        }

        private void ChangeDateTime()
        {
            if (Header.ScheduleType == "W" || Header.ScheduleType == "PT")
            {
                DateTimeFormat = "HH:mm:ss";
            }
            else
            {
                DateTimeFormat = "yyyy-MM-ddTHH:mm:ss";
            }
        }

        public void EditSchedule(Schedule sc)
        {
            Header.Description = sc.description;
            Header.Name = sc.name;
            Header.Recycle = sc.recycle;
            Header.Autodelete = sc.autodelete;
            Header.Enabled = sc.status;
            //sc.command.address.objecttype

            if (sc.localtime.Contains("PT"))
            {
                Header.ScheduleType = "PT";
                Regex timerRegex = new Regex(@"(R(\d\d)//?)?PT(\d\d:\d\d:\d\d)(A(\d\d:\d\d:\d\d)?)?");
                Match mc = timerRegex.Match(sc.localtime);
                Header.Datetime = DateTime.Parse(mc.Groups[3].Value);

                if (mc.Groups[2].Value != string.Empty)
                {
                    Repetitions = Convert.ToInt32(mc.Groups[2].Value);
                }

                if(mc.Groups[4].Value != string.Empty)
                {
                    Header.Randomize = true;
                }
            }
            else if (sc.localtime.Contains("W"))
            {
                Header.ScheduleType = "W";
                Regex alarmRegex = new Regex(@"(W(\d\d\d)//?)?T(\d\d:\d\d:\d\d)(A(\d\d:\d\d:\d\d))?");
                Match mc = alarmRegex.Match(sc.localtime);
                if(mc.Groups[2].Value != string.Empty)
                {
                    ScheduleMask = mc.Groups[2].Value;
                }

                if (mc.Groups[5].Value != string.Empty)
                {
                    Header.Randomize = true;
                }
            }
            else
            {
                Header.ScheduleType = "T";
                Regex scheduleRegex = new Regex(@"(.*)(A(\d\d:\d\d:\d\d)?)?");
                Match mc = scheduleRegex.Match(sc.localtime);
                Header.Datetime = DateTime.Parse(mc.Groups[1].Value, CultureInfo.InvariantCulture);

                if (mc.Groups[3].Value != string.Empty)
                {
                    Header.Randomize = true;
                }
            }

            if (sc.command?.address?.objecttype == null) return;

            switch (sc.command.address.objecttype)
            {
                case "lights":
                    Content = ContentTypeVm.Light;
                    break;
                case "groups":
                    Content = ContentTypeVm.Group;
                    break;
                case "schedule":
                    Content = ContentTypeVm.Schedule;
                    break;
                case "scene":
                    Content = ContentTypeVm.Schedule;
                    break;
                case "sensor":
                    Content = ContentTypeVm.Sensor;
                    break;
                default:
                    break;
            }

            SelectedTarget = _listTargetHueObject.FirstOrDefault(x => x.Id == sc.command.address.id);

            if (SelectedTarget == null)
            {
                MessageBox.Show(GlobalStrings.Object_Does_Not_Exists, GlobalStrings.Error, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            else
            {
                

            }
        }

        private void SelectTarget()
        {

            switch (SelectedTarget)
            {
                case Sensor _:
                {
                    ScheduleCreatorPropertyGridViewModel _scvm = _selectedViewModel as ScheduleCreatorPropertyGridViewModel;
                    _scvm.SelectedObject = HueSensorStateFactory.CreateSensorStateFromSensorType(((Sensor)SelectedTarget).type);
                    break;
                }
                case Schedule _:
                {
                    ScheduleCreatorPropertyGridViewModel _scvm = _selectedViewModel as ScheduleCreatorPropertyGridViewModel;
                    _scvm.SelectedObject = new Schedule();
                    break;
                }
                case Light _:
                case Group _:
                {
                    ScheduleCreatorPropertyGridViewModel _scvm = _selectedViewModel as ScheduleCreatorPropertyGridViewModel;
                    _scvm.SelectedObject = new State();
                    break;
                }
            }

            if (SelectedTarget == null) return;

            AdrTarget = new HueAddress();
            AdrTarget.api = "api";
            AdrTarget.key = _bridge.ApiKey;

            switch (_content)
            {
                case ContentTypeVm.Light:
                    AdrTarget.objecttype = "lights";
                    AdrTarget.property = "state";
                    AdrTarget.id = SelectedTarget.Id;                   
                    break;
                case ContentTypeVm.Group:
                    AdrTarget.objecttype = "groups";
                    AdrTarget.property = "action";
                    AdrTarget.id = SelectedTarget.Id;
                    break;
                case ContentTypeVm.Sensor:
                    AdrTarget.objecttype = "sensors";
                    AdrTarget.id = SelectedTarget.Id;
                    AdrTarget.property = "state";
                    break;
                case ContentTypeVm.Schedule:
                    AdrTarget.objecttype = "schedules";
                    AdrTarget.id = SelectedTarget.Id;
                    break;
                case ContentTypeVm.Scene:
                    AdrTarget.objecttype = "groups";
                    AdrTarget.id = "0";
                    AdrTarget.property = "action";
                    break;
                default:
                    break;
            }
            
        }

        public ScheduleCreatorViewModel()
        {
            ListTargetHueObject = new ObservableCollection<IHueObject>();
            _header = new ScheduleCreatorHeader();
            _selectedViewModel = new ScheduleCreatorSlidersViewModel();
            _content = ContentTypeVm.Light;
            _effect = "none";
            _dateTimeFormat = "yyyy-MM-ddTHH:mm:ss";
            _smask = "000";
        }

        public async Task Initialize(Bridge bridge, Schedule schedule = null)
        {
            _bridge = bridge;
            _currentHueObjectList = await HueObjectHelper.GetBridgeDataStoreAsyncTask(_bridge);
            
            if (_currentHueObjectList == null) return;
            ListTargetHueObject.AddRange(_currentHueObjectList.Where(x => x is Light).ToList());

            if (schedule == null) return;

            Header.Autodelete = schedule.autodelete;
           // Header.Datetime = schedule.localtime;
            Header.Description = schedule.description;
            Header.Name = schedule.name;
         
        }


        public Schedule GetSchedule()
        {
            Schedule sc = new Schedule
            {
                name = Header.Name,
                autodelete = Header.Autodelete,
                description = Header.Description,
                recycle = Header.Recycle,
                status = Header.Enabled,
                localtime = BuildScheduleTime(),
                command = new Command
                {
                    address = AdrTarget,
                    method = "PUT",
                },
            };

            string body = string.Empty;
            
            if(_selectedViewModel is ScheduleCreatorPropertyGridViewModel)
            {
                ScheduleCreatorPropertyGridViewModel scpg = _selectedViewModel as ScheduleCreatorPropertyGridViewModel;
                body = Serializer.SerializeToJson(scpg.SelectedObject); 
            }
            else if(_selectedViewModel is ScheduleCreatorSlidersViewModel)
            {
                ScheduleCreatorSlidersViewModel scsv = _selectedViewModel as ScheduleCreatorSlidersViewModel;
                body = Serializer.SerializeToJson(scsv);
            }

            sc.command.body = body;

            return sc;
        }

        private string BuildScheduleTime()
        {
            string time = string.Empty;
            switch (Header.ScheduleType)
            {
                case "T":
                    time = Header.Datetime.ToString("yyyy-MM-ddTHH:mm:ss");
                    break;
                case "PT":
                    time = $"{Header.Datetime:HH:mm:ss}".Insert(0, "PT");
                    break;
                case "W":
                    time = $"{Header.Datetime:HH:mm:ss}".Insert(0, $"W{_smask:000}/T");
                    break;
                default:
                    break;
                
            }

            return time;
        }

        public ScheduleCreatorHeader Header
        {
            get => _header;
            set => SetProperty(ref _header,value);
        }

        public ValidatableBindableBase SelectedViewModel
        {
            get => _selectedViewModel;
            set => SetProperty(ref _selectedViewModel, value);
        }

        public ICommand ChangeContentCommand => new RelayCommand(param => ChangeContent());

        public ContentTypeVm Content { get => _content; set => SetProperty(ref _content,value); }

        public ObservableCollection<IHueObject> ListTargetHueObject
        {
            get => _listTargetHueObject;
            set => SetProperty(ref _listTargetHueObject,value);
        }

        public IHueObject SelectedTarget
        {
            get => _selectedHueObject;
            set => SetProperty(ref _selectedHueObject,value);
        }

        public string Effect
        {
            get => _effect;
            set => SetProperty(ref _effect,value);
        }

        public string DateTimeFormat
        {
            get => _dateTimeFormat;
            set => SetProperty(ref _dateTimeFormat,value);
        }

        public string ScheduleMask
        {
            get => _smask;
            set => SetProperty(ref _smask,value);
        }

        public int Repetitions
        {
            get => _repetitions;
            set => SetProperty(ref _repetitions,value);
        }

        public HueAddress AdrTarget { get => _adrTarget; set => SetProperty(ref _adrTarget,value); }

        public bool PropGridLG
        {
            get => _propGridLG;
            set => SetProperty(ref _propGridLG,value);
        }

        private void ChangeContent()
        {
            ListTargetHueObject.Clear();
            
            if (_currentHueObjectList == null) return;
            if((Content == ContentTypeVm.Light || Content == ContentTypeVm.Group) && !PropGridLG)
            {
                SelectedViewModel = new ScheduleCreatorSlidersViewModel();
            }
            else
            {
                SelectedViewModel = new ScheduleCreatorPropertyGridViewModel();
            }

            switch (Content)
            {
                case ContentTypeVm.Light:              
                    ListTargetHueObject.AddRange(_currentHueObjectList.Where(x => x is Light).ToList());
                    break;
                case ContentTypeVm.Group:
                    ListTargetHueObject.AddRange(_currentHueObjectList.Where(x => x is Philips_Hue.HueObjects.GroupObject.Group).ToList());
                    break;
                case ContentTypeVm.Schedule:
                    ListTargetHueObject.AddRange(_currentHueObjectList.Where(x => x is Schedule).ToList());
                    break;
                case ContentTypeVm.Sensor:
                    ListTargetHueObject.AddRange(_currentHueObjectList.Where(x => x is Sensor).Where(x => ((Sensor)x).type.Contains("CLIP")).ToList());
                    break;
                case ContentTypeVm.Scene:
                    ListTargetHueObject.AddRange(_currentHueObjectList.Where(x => x is Scene).ToList());
                    break;
                default:
                    break;
            }

        }

    }
}
