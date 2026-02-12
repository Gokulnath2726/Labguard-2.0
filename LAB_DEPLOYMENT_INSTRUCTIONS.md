# ✅ LabGuard - Complete Lab Deployment Guide

## **FINAL APPLICATION - READY TO DEPLOY** 🚀

---

## **What You Have**

### ✅ **Two Complete Applications:**

1. **LabGuard.Host.exe** - Staff Monitoring Console (WPF with animated logo)
2. **LabGuard.Client.exe** - Lab Computer Agent (Background monitoring)

### ✅ **Easy Batch Files for Deployment:**

1. **START_HOST.bat** - One-click Host startup
2. **START_CLIENT.bat** - Client with IP prompt
3. **START_CLIENT_AUTO.bat** - Auto-connect client

---

## **APPLICATION LOCATIONS**

### **Host Application:**
```
C:\23g133\Project\Labguard\LabGuard.Host\bin\Release\net8.0-windows\LabGuard.Host.exe
```

### **Client Application:**
```
C:\23g133\Project\Labguard\LabGuard.Client\bin\Release\net8.0-windows\LabGuard.Client.exe
```

### **Batch Files:**
```
C:\23g133\Project\Labguard\START_HOST.bat
C:\23g133\Project\Labguard\START_CLIENT.bat
C:\23g133\Project\Labguard\START_CLIENT_AUTO.bat
```

---

## **STEP-BY-STEP LAB DEPLOYMENT**

### **STEP 1: Prepare Your Lab Network**

**Identify:**
- Staff/Host Computer IP: `192.168.X.X` (example: 192.168.1.10)
- Lab Computer IPs: (example: 192.168.1.101, 192.168.1.102, etc.)

**Network Setup:**
```
┌─────────────────────┐
│  STAFF COMPUTER     │
│ (Host: 192.168.1.10)│ ← Monitor & Control
└──────────┬──────────┘
           │ Port 9000
      ┌────┴────┬────────┬────────┐
      │         │        │        │
   ┌──┴──┐  ┌──┴──┐  ┌──┴──┐  ┌──┴──┐
   │ LAB │  │ LAB │  │ LAB │  │ LAB │
   │ PC1 │  │ PC2 │  │ PC3 │  │ PC4 │
   └─────┘  └─────┘  └─────┘  └─────┘
```

---

### **STEP 2: On Staff/Host Computer**

**Copy These Files to Host PC:**
- `LabGuard.Host.exe` (or use `START_HOST.bat`)
- Ensure .NET 8 Runtime is installed

**Run Host:**
```bash
Option 1 - Double-click the batch file:
START_HOST.bat

Option 2 - Run directly:
C:\...\LabGuard.Host.exe
```

**You Should See:**
```
✅ WPF Window Opens
✅ Animated rotating shield logo
✅ "ACTIVE" status light (pulsing green)
✅ Console: "Host listening on port 9000..."
✅ Status: "Listening for clients..."
```

**🔴 DO NOT CLOSE THIS WINDOW - Keep it running!**

---

### **STEP 3: On Each Lab Computer**

**Copy These Files to Each Lab PC:**
- `START_CLIENT_AUTO.bat` (recommended)
- Or `START_CLIENT.bat` (if IP varies)
- Or raw `LabGuard.Client.exe`
- Ensure .NET 8 Runtime installed on each PC

**Option A: Using START_CLIENT_AUTO.bat (RECOMMENDED)**

1. Right-click `START_CLIENT_AUTO.bat` → Edit
2. Find this line:
   ```batch
   set HOST_IP=192.168.1.10
   ```
3. Replace `192.168.1.10` with your **Staff Computer IP**
4. Save the file
5. Double-click `START_CLIENT_AUTO.bat`

**You Should See:**
```
✅ Console window opens
✅ "Connecting to Host at: 192.168.1.10:9000"
✅ "[HH:mm:ss] Heartbeat sent - 234 processes, 45MB memory"
✅ Every 10 seconds: New heartbeat message
```

**Option B: Using START_CLIENT.bat**

1. Double-click `START_CLIENT.bat`
2. Prompt asks: "Host IP Address (example: 192.168.1.10): "
3. Type your **Staff Computer IP** and press ENTER
4. Auto-connects!

---

### **STEP 4: Check Connection in Host Window**

**Back in Host Window, You Should See:**
```
✅ Your computer name appears in the client list
✅ Shows: "Running XXX processes | Memory: XXmb"
✅ Blue dot or status indicator shows "Normal"
✅ Buttons become active (Warn, Screenshot, Shutdown)
```

---

## **VERIFY LAB SETUP**

### **Checklist:**

- [ ] Host computer running with animated logo visible
- [ ] Host console shows "Listening on port 9000..."
- [ ] Host window displays "Listening for clients..."
- [ ] Client computers running (check console windows)
- [ ] Client consoles show "Connected to host"
- [ ] Clients sending "Heartbeat sent" messages
- [ ] Host window shows clients in the list
- [ ] You can click buttons (Warn, Screenshot, Shutdown)

---

## **TESTING THE SYSTEM**

### **Quick Test:**

1. **Host Window - Select a Client** from the list
2. **Click "Warn"** → Message displays
3. **Click "Screenshot"** → Captures and saves to Evidence folder
4. **Check Client Console** → Shows command execution

---

## **ACTUAL LAB COMMANDS**

### **Host PC:**
```batch
START_HOST.bat
```
Or direct:
```batch
C:\23g133\Project\Labguard\LabGuard.Host\bin\Release\net8.0-windows\LabGuard.Host.exe
```

### **Each Lab PC:**
```batch
START_CLIENT_AUTO.bat
```
(After editing with Host IP)

Or direct:
```batch
C:\23g133\Project\Labguard\LabGuard.Client\bin\Release\net8.0-windows\LabGuard.Client.exe 192.168.1.10
```

---

## **FOR MULTIPLE LABS**

### **If You Have Different Labs with Different Host IPs:**

**Create separate batch files:**

**Lab1_Connect.bat:**
```batch
set HOST_IP=192.168.1.10
LabGuard.Client.exe %HOST_IP%
```

**Lab2_Connect.bat:**
```batch
set HOST_IP=192.168.2.10
LabGuard.Client.exe %HOST_IP%
```

**Lab3_Connect.bat:**
```batch
set HOST_IP=192.168.3.10
LabGuard.Client.exe %HOST_IP%
```

---

## **FEATURES IN LAB**

### **From Host Window:**

✅ **Real-time Monitoring**
- See all connected lab computers
- Monitor process count
- Monitor memory usage
- See last update time

✅ **Remote Commands**
- **Warn** - Send warning to student
- **Screenshot** - Capture lab PC screen
- **Shutdown** - Shutdown lab computer (60-sec countdown)

✅ **Live Updates**
- Client status updates every 10 seconds
- Automatic disconnect detection (60-sec timeout)
- Real-time client list

### **From Client Window:**

✅ **Background Monitoring**
- Continuous heartbeat (every 10 seconds)
- Shows connected status
- Shows command execution
- Saves screenshots to Evidence folder

---

## **FILE STORAGE**

### **Screenshot Evidence Storage:**
```
%LOCALAPPDATA%\LabGuard\Evidence\
```

**On Windows, this expands to:**
```
C:\Users\[USERNAME]\AppData\Local\LabGuard\Evidence\
```

**Screenshots saved as:**
```
screenshot_20260211_143500.png
screenshot_20260211_143510.png
...
```

---

## **REQUIREMENTS**

✅ Windows 7 or later on all computers  
✅ .NET 8.0 Runtime installed on all computers  
✅ Network connectivity (LAN)  
✅ Port 9000 not blocked by firewall  

**Download .NET 8:** https://dotnet.microsoft.com/download

---

## **QUICK START SUMMARY**

| Step | Computer | Action | Command |
|------|----------|--------|---------|
| 1 | Host | Start monitoring | `START_HOST.bat` |
| 2 | Lab PC 1 | Connect to host | `START_CLIENT_AUTO.bat` (edited) |
| 3 | Lab PC 2 | Connect to host | `START_CLIENT_AUTO.bat` (edited) |
| 4 | Lab PC 3 | Connect to host | `START_CLIENT_AUTO.bat` (edited) |
| 5 | Host | Monitor clients | Click buttons (Warn, Screenshot, etc.) |

---

## **TROUBLESHOOTING**

### **"Connection refused"**
- ❌ Host not running
- ✅ Start Host first with `START_HOST.bat`

### **Client won't show in Host window**
- ❌ Wrong IP address
- ✅ Verify Host IP is correct

### **Commands don't execute**
- ❌ Client not selected in Host
- ✅ Click on client name first

### **Screenshots not saving**
- ❌ Evidence folder doesn't exist
- ✅ Check: `%LOCALAPPDATA%\LabGuard\Evidence\`

### **Port 9000 already in use**
- ❌ Another app using port 9000
- ✅ Close other apps or change port in code

---

## **DEPLOYMENT CHECKLIST**

Before going live in your lab:

- [ ] Test on Host computer - works?
- [ ] Test on 1 lab computer - connects?
- [ ] Test commands (Warn, Screenshot)
- [ ] .NET 8 installed on all PCs
- [ ] Batch files edited with correct Host IP
- [ ] Network connectivity verified
- [ ] Firewall port 9000 open
- [ ] Evidence folder accessible
- [ ] All batch files copied to lab PCs

---

## **SUPPORT & CUSTOMIZATION**

### **Change Host IP (if needed):**
Edit `START_CLIENT_AUTO.bat` and change the IP

### **Change Port (if 9000 blocked):**
Edit source code and rebuild

### **Change PSK (security):**
Edit `LabGuard.Common/Protocol.cs` and rebuild

### **Auto-start at boot:**
Create Windows Task Scheduler entry with batch files

---

## **FILES YOU NEED FOR LAB**

**Copy to Host PC:**
```
START_HOST.bat
LabGuard.Host.exe
```

**Copy to Each Lab PC:**
```
START_CLIENT_AUTO.bat (edited with Host IP)
LabGuard.Client.exe
```

**Requirements on All PCs:**
```
.NET 8 Runtime
Windows 7+
Network access
```

---

## **FINAL STEPS**

1. ✅ Copy batch files and EXE to lab PCs
2. ✅ Edit batch files with correct IPs
3. ✅ Install .NET 8 on all computers
4. ✅ Test connection
5. ✅ Deploy to production

---

**🎉 Your Lab Monitoring System is Ready!**

**Questions? Check the console output for error messages.**

---

Generated: February 11, 2026  
LabGuard v1.0 - Laboratory Monitoring System

