# Setup

#### Step 1: Download the Project

#### Step 2: Open in Unity

#### Step 3: Customize the UI to your requirements, though these can be edited remotely too!

#### Step 4: The Remote Config and Authentication packages should already be installed but if they aren't go ahead and install them

#### Step 5: Go to the [Unity Cloud Dashboard](https://cloud.unity.com/) and click on the Dashboard tab. Sign in/up if necessary
![[Cloud Dashboard.png]]

### Step 6: Create a new Project if you don't have one
![[Create new Project.png]]

### Step 7: Enter the Project View
![[Click on created project.png]]

### Step 8: Launch Remote Config under Services
![[Remote Config Service.png]]

### Step 9: You fill find some pop-ups and on-boarding steps. Click on Check Environements
![[Setup Guide.png]]

### Step 10: Create new environments or just choose Confirm to continue with the Production environment
![[Manage Environemtns.png]]

### Step 11: Click on the Drop Down with the chosen environment
![[Manage Environment.png]]

### Step 12: Click on Manage Environments and Scroll down till you find the Environment ID
![[Environement ID.png]]

### Step 13: Copy this ID

### Step 14: Go back to the Config tab on the Remote Config Service
![[Remote Config Config.png]]

### Step 15: Now visit the Unity editor and Click on the Launcher GameObject to edit it in the inspector and Check Out the Remote Config Variables
![[Remote Config variables in unity.png]]

### Step 16: Enter the previously copied ID in the Environement ID slot and configure the variable names for remote config as you wish

### Step 17: Create all the variables in the actual Remote Config dashboard as "Keys" with their correct values (What [values](./Files.md)?)
![[Add key.png]]
![[Key details.png]]
![[Complete Keys List.png]]

### Step 18: Publish
![[Publish Keus.png]]

### Step 19: Go to the main project view which we visit at Step 7 
### Step 20: Go back to the Unity Editor and Click on the Services option in the title ribbon
![[Services.png]]

### Step 21: Go to General Settings and choose your Organisation and Project and click on "Link Unity project to cloud project"
![[Enter details.png]]
![[Project all set up.png]]

### Step 22: ANDDDD you are done! Export the project to all required platforms and it 'should' work perfectly fine