using System;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace Frax2
{
	public enum TextColors {
		Blue = 1,
		Red,
		Green
	};

	public enum BackgroundColors {
		Black = 1,
		White,
		Blue,
		Pattern
	};

	/// <summary>
	/// This class manages the system settings.
	/// </summary>
	public class Settings
	{
		public static string FirstName { get; private set;}
		public static string LastName { get; private set;}
		public static int Iterations { get; private set;}
		public static BackgroundColors BackgroundColor { get; private set;}

		const string firstNameKey = "firstNameKey";
		const string lastNameKey = "lastNameKey";
		const string iterationsKey = "iterationsKey";
		const string backgroundColorKey = "backgroundColorKey";

		static void LoadDefaultValues ()
		{
			var settingsDict = new NSDictionary (NSBundle.MainBundle.PathForResource ("Settings.plist", null));

			var prefSpecifierArray = settingsDict[(NSString)"PreferenceSpecifiers"] as NSArray;

			foreach (var prefItem in NSArray.FromArray<NSDictionary> (prefSpecifierArray)) {
				var key = prefItem[(NSString)"Key"] as NSString;
				if (key == null)
					continue;
				var val = prefItem[(NSString)"DefaultValue"];
				switch (key.ToString ()) {
				case firstNameKey:
					FirstName = val.ToString ();
					break;
				case lastNameKey:
					LastName = val.ToString ();
					break;
				case iterationsKey:
					Iterations =  ((NSNumber)val).Int32Value;
					break;
				case backgroundColorKey:
					BackgroundColor =  (BackgroundColors)((NSNumber)val).Int32Value;
					break;
				}
			}
			var appDefaults = NSDictionary.FromObjectsAndKeys (new object[] {
				new NSString (FirstName),
				new NSString (LastName),
				new NSNumber ((int)Iterations),
				new NSNumber ((int)BackgroundColor)
			},
				new object [] { firstNameKey, lastNameKey, iterationsKey, backgroundColorKey }
			);

			NSUserDefaults.StandardUserDefaults.RegisterDefaults (appDefaults);
			NSUserDefaults.StandardUserDefaults.Synchronize ();
		}

		public static void SetupByPreferences ()
		{
			var testValue = NSUserDefaults.StandardUserDefaults.StringForKey (firstNameKey);
			if (testValue == null)
				LoadDefaultValues ();
			FirstName = NSUserDefaults.StandardUserDefaults.StringForKey (firstNameKey);
			LastName = NSUserDefaults.StandardUserDefaults.StringForKey (lastNameKey);
			Iterations = NSUserDefaults.StandardUserDefaults.IntForKey (iterationsKey);
			BackgroundColor = (BackgroundColors)NSUserDefaults.StandardUserDefaults.IntForKey (backgroundColorKey);
		}
	}

	public class SettingsUI 
	{
		public DialogViewController myController;
		public UINavigationController navigation;

		public SettingsUI() {
			myController = viewController ();
			navigation = new UINavigationController ();
			navigation.PushViewController (myController, true);				
		}

		private DialogViewController viewController()
		{
			return new DialogViewController (rootElement(), true) {
				//Style = UITableViewStyle.Grouped;
			};
		}

		private RootElement rootElement() 
		{
			return new RootElement ("Settings") {
				new Section () {
					new BooleanElement ("Airplane Mode", false),
					new RootElement ("Notifications", 0, 0) {
						new Section (null, 
							"Turn off Notifications to disable Sounds\n" +
							"Alerts and Home Screen Badges for the\napplications below.") {
							new BooleanElement ("Notifications", false)
						}
					}
				},
				new Section () {
					new RootElement ("Sounds") {
						new Section ("Silent") {
							new BooleanElement ("Vibrate", true),
						},
						new Section ("Ring") {
							new BooleanElement ("Vibrate", true),
							new FloatElement (null, null, 0.8f),
							new RootElement ("Ringtone", new RadioGroup (0)) {
								new Section ("Custom") {
									new RadioElement ("Circus Music"),
									new RadioElement ("True Blood"),
								},
								new Section ("Standard") {
									from name in "Marimba,Alarm,Ascending,Bark".Split (',')
									select (Element)new RadioElement (name)
								}
							},
							new RootElement ("New Text Message", new RadioGroup (3)) {
								new Section () {
									from name in "None,Tri-tone,Chime,Glass,Horn,Bell,Electronic".Split (',')
									select (Element)new RadioElement (name)
								}
							},
							new BooleanElement ("New Voice Mail", false),
							new BooleanElement ("New Mail", false),
							new BooleanElement ("Sent Mail", true),
						}
					},
					new RootElement ("Brightness") {
						new Section () {
							new FloatElement (null, null, 0.5f),
							new BooleanElement ("Auto-brightness", false),
						}
					}
				},
				new Section () {
					new EntryElement ("Login", "Your login name", "miguel"),
					new EntryElement ("Password", "Your password", "password", true),
					new DateElement ("Select Date", DateTime.Now),
				}
			};		
		}
	}
}

