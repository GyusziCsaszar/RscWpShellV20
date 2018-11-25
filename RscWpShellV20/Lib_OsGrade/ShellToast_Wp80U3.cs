using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Microsoft.Phone.Shell;

namespace OsGrade
{
	
	public static class ShellToast_Wp80U3 
	{
		
		// Set the minimum version number that supports custom toast sounds
		private static Version TargetVersion = new Version(8, 0, 10492);
				
		// Function to determine if the current device is running the target version.
		public static bool IsTargetedVersion { get { return Environment.OSVersion.Version >= TargetVersion; } }
			
		// Function for setting a property value using reflection.
		private static void SetProperty(object instance, string name, object value)
		{
			var setMethod = instance.GetType().GetProperty(name).GetSetMethod();
			setMethod.Invoke(instance, new object[] { value });
		}		
		
		public static void ShowToast( string title, string content, Uri sound ) //bool useCustomSound, /*bool useWavFormat,*/ bool doSilentToast)
		{
			ShellToast toast = new ShellToast();
			toast.Title = title;
			toast.Content = content;
		
			//If the device is running the right version and a custom sound is requested
			if ((IsTargetedVersion)) // && (useCustomSound))
			{
				/*
				if (useWavFormat)
				{
					//Do the reflection to get the new Sound property added to the toast
					SetProperty(toast, "Sound", new Uri("MyToastSound.wav", UriKind.RelativeOrAbsolute));
				}
				else
				*/
				{
					//Do the reflection to get the new Sound property added to the toast
					//SetProperty(toast, "Sound", Uri( "MyToastSound.mp3", UriKind.Relative ));
					
					SetProperty(toast, "Sound", sound);
				}
			}
			/*
			// For a silent toast, check the version and then set the Sound property to an empty string.
			else if ((IsTargetedVersion) && (doSilentToast))
			{
				//Do the reflection to get the new Sound property added to the toast
				SetProperty(toast, "Sound", new Uri("", UriKind.RelativeOrAbsolute));
			}
			*/
		
			toast.Show();
		}
		
	}
	
}