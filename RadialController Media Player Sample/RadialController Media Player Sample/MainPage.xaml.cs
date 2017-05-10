﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Input;
using Windows.Devices.Haptics;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RadialController_Media_Player_Sample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        RadialController myController;
        RadialControllerConfiguration config;
        RadialControllerMenuItem volumeItem;
        RadialControllerMenuItem playbackItem;
        enum Mode { Volume, Playback };
        Mode currentMode;
        bool doNotProcessClick = false;

        public MainPage()
        {
            this.InitializeComponent();

            // Create a reference to the RadialController.
            myController = RadialController.CreateForCurrentView();
            myController.RotationResolutionInDegrees = 5;
            myController.UseAutomaticHapticFeedback = false;

            // Create menu items for the custom tool.
            volumeItem = RadialControllerMenuItem.CreateFromFontGlyph("Volume", "\xE767", "Segoe MDL2 Assets");
            playbackItem = RadialControllerMenuItem.CreateFromFontGlyph("Playback", "\xE714", "Segoe MDL2 Assets");

            volumeItem.Invoked += VolumeItem_Invoked;
            playbackItem.Invoked += PlaybackItem_Invoked;

            //Add the custom tool's menu item to the menu
            myController.Menu.Items.Add(volumeItem);
            myController.Menu.Items.Add(playbackItem);
            
            //Create handlers for button and rotational input
            myController.RotationChanged += MyController_RotationChanged;
            myController.ButtonClicked += MyController_ButtonClicked;

            //Remove system's built-in tools
            config = RadialControllerConfiguration.GetForCurrentView();
            config.SetDefaultMenuItems(new RadialControllerSystemMenuItemKind[] { });

            //Set up menu suppression targets

            config.ActiveControllerWhenMenuIsSuppressed = myController;
            myController.ButtonHolding += MyController_ButtonHolding;

            myPlayer.CurrentStateChanged += MyPlayer_CurrentStateChanged;


            //select the first tool
            myPlayer.Loaded += MyPlayer_Loaded;
        }

        private void MyPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            myController.Menu.SelectMenuItem(playbackItem);
        }

        private void PlaybackItem_Invoked(RadialControllerMenuItem sender, object args)
        {
            currentMode = Mode.Playback;
        }

        private void VolumeItem_Invoked(RadialControllerMenuItem sender, object args)
        {
            currentMode = Mode.Volume;
        }

        private void MyController_ButtonClicked(RadialController sender, RadialControllerButtonClickedEventArgs args)
        {
            if (doNotProcessClick)
            {
                doNotProcessClick = false;
                return;
            }

            if (currentMode == Mode.Playback)
            {
                if (myPlayer.CurrentState == MediaElementState.Playing)
                {
                    myPlayer.Pause();
                }
                else
                {
                    myPlayer.Play();
                }
            }
            else if(currentMode == Mode.Volume)
            {
                myPlayer.IsMuted = !myPlayer.IsMuted;
            }

        }

        private void MyPlayer_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            if (myPlayer.CurrentState == MediaElementState.Playing)
            {
                config.IsMenuSuppressed = true;
            }
            else
            {
                config.IsMenuSuppressed = false;
            }
        }

        private void MyController_ButtonHolding(RadialController sender, RadialControllerButtonHoldingEventArgs args)
        {
            if (currentMode == Mode.Playback)
            {
                myController.Menu.SelectMenuItem(volumeItem);
            }
            else if(currentMode == Mode.Volume)
            {
                myController.Menu.SelectMenuItem(playbackItem);
            }

            SendBuzzFeedback(args.SimpleHapticsController);
            doNotProcessClick = true;
        }

        private void MyController_RotationChanged(RadialController sender, RadialControllerRotationChangedEventArgs args)
        {
            if (myController.Menu.GetSelectedMenuItem().Equals(playbackItem))
            {
                myPlayer.Position = myPlayer.Position + TimeSpan.FromSeconds(args.RotationDeltaInDegrees);
            }
            else
            {
                myPlayer.Volume += args.RotationDeltaInDegrees / 100;
            }
        }

        private void SendBuzzFeedback(SimpleHapticsController hapticController)
        {
            var feedbacks = hapticController.SupportedFeedback;

            foreach (SimpleHapticsControllerFeedback feedback in feedbacks)
            {
                if (feedback.Waveform == KnownSimpleHapticsControllerWaveforms.Click)
                {
                    hapticController.SendHapticFeedbackForPlayCount(feedback, 1, 3, TimeSpan.FromMilliseconds(250));
                    return;
                }
            }
        }
    }
}
