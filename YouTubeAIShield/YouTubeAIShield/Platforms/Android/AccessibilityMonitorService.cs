using Android.AccessibilityServices;
using Android.Views.Accessibility;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Util;
using Android.App;
using Android.Runtime;
using AColor = Android.Graphics.Color;
using AView = Android.Views.View;
using Android.OS;

namespace YouTubeAIShield.Platforms.Android;

[Service(
    Name = "com.companyname.youtubeaishield.AccessibilityMonitorService",
    Permission = "android.permission.BIND_ACCESSIBILITY_SERVICE",
    Exported = true)]
public class AccessibilityMonitorService : AccessibilityService
{
    const string _tag = "AIShield";
    AView warningView;
    IWindowManager windowManager;

    public override void OnAccessibilityEvent(AccessibilityEvent e)
    {

        Log.Debug(_tag, "Accessibility event triggered");

        if (e == null)
        {
            return;
        }

        if (e.PackageName.ToString() != "com.google.android.youtube") //We only care about the application YouTube
        {
            return;
        }

        Log.Debug(_tag, $"Event detected: {e.EventType}");

        if (e.EventType != EventTypes.WindowStateChanged &&
        e.EventType != EventTypes.WindowContentChanged)
        {
            return;
        }

        var root = RootInActiveWindow;
        if (root == null)
        {
            return;
        }

        ScanNode(root);
    }

    protected override void OnServiceConnected()
    {
        base.OnServiceConnected();
        Log.Debug("AIShield", "Accessibility service connected!");
    }


    void ScanNode(AccessibilityNodeInfo node) 
    {
        if (node == null)
            return;

        if (node.Text != null)
        {
            string text = node.Text.ToString();

            Log.Debug(_tag, $"Screen text: {text}");

            CheckForAIVideo(text);
        }

        for (int i = 0; i < node.ChildCount; i++)
        {
            ScanNode(node.GetChild(i));
        }
    }

    void CheckForAIVideo(string text)
    {
        string[] aiKeywords =
        {
        "ai generated",
        "chatgpt",
        "midjourney",
        "ai voice",
        "ai story",
        "ai images",
        "ai video",
        "ai"
    };
        string shortText = text.Length > 50 ? text.Substring(0, 50) + "..." : text;


        if (text == "Shorts")
        {
            Log.Debug(_tag, "User is viewing Shorts");
        }

        foreach (var keyword in aiKeywords)
        {
            if (text.ToLower().Contains(keyword))
            {
                Log.Warn(_tag, $"⚠ Possible AI video detected: {text}");

                ShowWarningOverlay($"⚠ Possible AI Generated Video\nDetected in: {shortText}");
            }
        }
    }

    void ShowWarningOverlay(string message)
    {
        if (warningView != null)
            return; // prevent multiple overlays

        windowManager = GetSystemService(WindowService).JavaCast<IWindowManager>();

        var textView = new TextView(this);
        textView.Text = message;
        textView.SetBackgroundColor(AColor.Red);
        textView.SetTextColor(AColor.White);
        textView.TextSize = 22;
        textView.SetPadding(40, 40, 40, 40);

        var layoutParams = new WindowManagerLayoutParams(
            ViewGroup.LayoutParams.MatchParent,
            ViewGroup.LayoutParams.WrapContent,
            WindowManagerTypes.ApplicationOverlay,
            WindowManagerFlags.NotFocusable,
            Format.Translucent);

        layoutParams.Gravity = GravityFlags.Top;

        warningView = textView;

        windowManager.AddView(warningView, layoutParams);

        // Remove overlay after 5 seconds
        new Handler(MainLooper).PostDelayed(() =>
        {
            if (warningView != null)
            {
                Log.Debug(_tag, "Closing Banner");
                windowManager.RemoveView(warningView);
                warningView = null;
            }

        }, 5000);
    }

    public override void OnInterrupt()
    {
        Log.Debug(_tag, "Accessibility service interrupted.");
    }
}
