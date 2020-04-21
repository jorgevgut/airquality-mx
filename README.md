# AirQualityMX project: air quality notifications for Mexico

## What is this? Why?

This can be called a 'personal' project by [@jorge\_vgut](https://twitter.com/jorge_vgut) with the original intentions of learning C# Programming language, and at the same time, do something that could be shared and used to share knowledge on programming and software design. If you'd like to learn about how this project is design, please check our [high level design document](https://github.com/jorgevgut/airquality-mx/wiki/High-level-System-Design) first.

## Project status
**This project is in DEVELOPMENT** currently undergoing tests.**
If you would like to learn more about this, please head to our [Wiki on Github](https://github.com/jorgevgut/airquality-mx/wiki).



## Coding guidelines
Intention is to follow Microsoft's dotnet best practices.
Visit their dotnet/runtime project on Github.
https://github.com/dotnet/runtime/blob/master/docs/coding-guidelines/coding-style.md

Use CodeFormatter: https://github.com/dotnet/codeformatter
as a tool to format code.

**Note: code formatting and perfect style is out of scope for the moment**

## Instructions

### Deploy cloud infrastructure on AWS
Go to cloud-infra/AirQualityCDK/ which is an AWS CDK project, here you will need the following requirements.
- Need an Aws account with permissions to deploy all services, for details see the CDK Stack implementation in this project.
- Have aws cli installed and configuration already set locally for cdk deploy to pick it up.
- Set the following environment variables set
    ```
    # may put this values on env-setup.sh for easing the process
    TOKEN -> Waqi API token
    TWITTER_APIK=Twitter API KEY;
    TWITTER_APIS=Twitter API Secret;
    TWITTER_ACCESS_TOKEN=Twitter access token (for development purposes);
    TWITTER_SECRET_TOKEN=Twitter secret token (for development purposes);
    ```
- Will need S3 bucket to hold package lambdas. Refer to script in `src/Aws/package-and-upload-lambdas.sh`
- project is a dotnetcore v3 app, make sure to have dotnet cli installed to build the project.

Sample setup:
```
#from /project-root
# make sure all projects build and are tested(test is very limited), this is done automatically as all are added as part of solution in project root.
dotnet build
dotnet test

# from src/Aws
# this packages and upload lambda zip files into s3 buckets, this will be picked up by CDK stack
./package-and-upload-lambdas.sh

# from cloud-infra/AirQualityCDK
# setup required environment variables so CDK provisions resources with these as environment variables
./env-setup.sh
# provision everython on Aws as a Stack
cdk deploy

```

## Contributions

Should you like to provide any feedback, please open up an Issue, I appreciate feedback and comments, although please keep in mind the project is incomplete, and I'm doing my best to keep it up to date.

Currently the project is NOT ACCEPTING CODE CONTRIBUTIONS (pull requests, or else)
However as this is in its early stages, you are welcome to leave feedback on its current design.
