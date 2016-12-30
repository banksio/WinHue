﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using HueLib2;
using WinHue3.ViewModels;

namespace WinHue3
{
    /// <summary>
    /// Interaction logic for Form_ResourceLinksCreator.xaml
    /// </summary>
    public partial class Form_ResourceLinksCreator : Window
    {
        private ResourceLinkCreatorViewModel rlcvm;
        public Form_ResourceLinksCreator()
        {
            InitializeComponent();
            rlcvm = this.DataContext as ResourceLinkCreatorViewModel;
            HelperResult hr = HueObjectHelper.GetBridgeDataStore(BridgeStore.SelectedBridge);
            if (hr.Success)
            {
                List<HueObject> _listbrobj = (List<HueObject>) hr.Hrobject;
                foreach (var l in _listbrobj)
                {
                    if(!(l is Resourcelink))
                        rlcvm.ListHueObjects.Add(l);
                }
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(rlcvm.ListHueObjects);
                view.GroupDescriptions?.Clear();
                PropertyGroupDescription groupDesc = new TypeGroupDescription();
                view.GroupDescriptions?.Add(groupDesc);

                CollectionView view2 = (CollectionView)CollectionViewSource.GetDefaultView(rlcvm.LinkCreatorModel.ListlinkObject);
                view2.GroupDescriptions?.Clear();
                PropertyGroupDescription groupDesc2 = new TypeGroupDescription();
                view2.GroupDescriptions?.Add(groupDesc2);
            }
            
        }

        private void btnCreateResourceLink_Click(object sender, RoutedEventArgs e)
        {
            Resourcelink rl = rlcvm.Resourcelink;
        }

  

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
