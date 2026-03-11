using System;
using System.Collections.Generic;
using System.Text;
using Android.AccessibilityServices;
using Android.Views.Accessibility;
using Android.Util;
using Android.App;

namespace YouTubeAIShield;

[Service(
    Permission = "android.permission.BIND_ACCESSIBILITY_SERVICE",
    Exported = true)]
public class AccessibilityMonitorService : AccessibilityService
{
    const string _tag = "AIShield";

    public override void OnAccessibilityEvent(AccessibilityEvent e)
    {
        if(e == null)
        {
            return;
        }

        if (e.PackageName.ToString() != "com.google.android.youtube") //We only care about the application YouTube
        {
            return;
        }

        Log.Debug(_tag, $"Event detected: {e.EventType}");

        var root = RootInActiveWindow;
        if (root == null)
        {
            return;
        }

        ScanNode(root);
    }

    void ScanNode(AccessibilityNodeInfo node) 
    {
        if (node == null) {
            return;
        }
        if (node.Text != null)
        {
            String text = node.Text.ToString();
            Log.Debug(_tag, $"Screen text: {text}");
        }

        for (int i = 0; i < node.ChildCount; i++)
        {
            ScanNode(node.GetChild(i));
        }
    }

    public override void OnInterrupt()
    {
        Log.Debug(_tag, "Accessibility service interrupted.");
    }
}
