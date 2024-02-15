# Hosting
You can host the files on and http hosting service like dropbox, google drive or even onedrive. You would need to convert the links to direct download links using some kind of online converters. 
For example this is a direct download link for a dropbox file `https://www.dropbox.com/s/SOME_ID/Application_Zip?dl=1`

Google Drive shows a pop-up if the file is more than 50mb which breaks the download http request. You can either use [Google APIs](https://bytesbin.com/skip-google-drive-virus-scan-warning-large-files/) to get rid of the whole problem altogether... Or you could even do a [workaround via code](https://stackoverflow.com/a/44402826)

So the Remote Config variables would point to these variables... Here is an example (acknowledgement is basically subtitle.. I renamed it for my project)

### Also the \_name variables should be equal the name for the Zip files
![image](https://github.com/AXVIII3/unity-app-launcher/assets/76608488/59fab93f-c0f6-4470-bb5b-0a8e11462694)


# File Structure
The launcher detects its current directory and creates a `Data` folder on the same level as itself... The zip is downloaded and extracted inside this folder...<br>
![image](https://github.com/AXVIII3/unity-app-launcher/assets/76608488/24175a3a-f980-453c-a2dc-7571b9bd0166)


# Compression
After compression, decompress the file and choose normal extraction (Not extract here or extract at 'X'). If the extracted file shows the game files directly under a folder with the Directory name then it would work fine. Though this can be edited in code if necessary.<br>
![image](https://github.com/AXVIII3/unity-app-launcher/assets/76608488/5ddc770e-6a31-4b91-8b58-d77505eed5f4)
