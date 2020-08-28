<div align="center">
  <img src="https://github.com/DigitalCitizen110625/SafariBugTracker-Public/blob/master/ReadmeAssests/SafariLogo.png">
</div>

<div align="center">
 
  [![Status](https://img.shields.io/badge/status-active-success.svg?style=for-the-badge)]() 
  [![License](https://img.shields.io/badge/license-MIT-blue.svg?style=for-the-badge)](/LICENSE)
  
</div>

## 📝 Table of Contents

* [Demo](https://safaribugtracker.azurewebsites.net/)
* [About](https://github.com/DigitalCitizen110625/SafariBugTracker-Public#elephantabout-safari-bug-tracker)
* [FAQ](https://github.com/DigitalCitizen110625/SafariBugTracker-Public#grey_questionfaq)
* [Software Requirment Specifications](https://github.com/DigitalCitizen110625/SafariBugTracker-Public/SRS/SafariBugTracker_SRS.docx)

## :cloud:	Live Demo
Please visit : <a href="https://safaribugtracker.azurewebsites.net/">SafariBugTracker</a> for a live demo of the system


## :elephant:	About Safari Bug Tracker
Safari is a cloud-based issue management system designed for small agile teams. It’s a free solution for QA staff, project managers, and developers seeking to store, and quantify software issues in a structured. It was developed in C#/.NET, using the ASP.NET Core framework.

For a breakdown of its features, subsystems, and technologies used, please see the <a href="https://safaribugtracker.azurewebsites.net/Home/About">About page</a>


## :grey_question:	FAQ
1. Will it run out of the box? <br/> Yes, but all connection strings, and authentication keys were removed from the public release, in order to keep them secure. If you wish to run the application on your own, you must have a SQL, Mongodb, Azure Table Storage, and AWS Simple Email Service set up. Then fill in the configurations in the appsettings.json file, as seen below:  <br/><br/>
  "IssueApiKey": {<br/>
    &nbsp;&nbsp;&nbsp; "ApiKey": "YOUR_NEW_KEY_HERE"<br/>
  }<br/> //This can be any string you want, but it must match the corresponding AuthKey in the IssueAPI appsettings.json file <br/><br/>
  "IssueRepositorySettings": {<br/>
    &nbsp;&nbsp;&nbsp; "BaseUri": "URL_WHERE_YOU_HOSTED_THE_ISSUE_API"<br/>
  }<br/> //This is the base URL of where to access the IssueAPI. For example: https://localhost:44371/api/ <br/><br/>
  "AzureTableSettings": {<br/>
    &nbsp;&nbsp;&nbsp; "AccountName": "YOUR_AZURE_STORAGE_ACCOUNT_NAME ",<br/>
    &nbsp;&nbsp;&nbsp; "SasToken": "YOUR_AUTO_GENERATED_TABLE_SAS_KEY",<br/>
    &nbsp;&nbsp;&nbsp; "TableName": "YOUR_TABLE_NAME "<br/>
  }<br/>//These must match with your Azure Storage account, especially the SAS token, which can be generated from the azure portal <br/><br/>
"SmtpEmailSettings": {<br/>
    &nbsp;&nbsp;&nbsp; "ToAddress": "ENDPOINT_EMAIL_ADDRESS", <br/>
    &nbsp;&nbsp;&nbsp; "FromAddress": "SENDER_EMAIL_ADDRESS",<br/>
    &nbsp;&nbsp;&nbsp; "FromName": "DISPLAY_NAME_OF_SENDER",<br/>
    &nbsp;&nbsp;&nbsp; "SmtpUserName": "SMTP_USERNAME",<br/>
    &nbsp;&nbsp;&nbsp; "SmtpPassword": "SMTP_PASSWORD",<br/>
    &nbsp;&nbsp;&nbsp; "Host": "SMTP_HOST",<br/>
    &nbsp;&nbsp;&nbsp; "Port": "SMTP_PORT"<br/>
  }<br/> //Fill in your credentials for your email service. Note that the app uses AWS SES, which requires an SmtpUserName, and SmtpPassword in order to access their resources<br/><br/>
  "ConnectionStrings": {<br/>
    &nbsp;&nbsp;&nbsp; "SafariBugTrackerWebAppContextConnection": "YOUR_SQL_DB_CONNECTION_STRING"<br/>
  }<br/> //Add a connection string to your SQL server where the user account data will be stored