### Set up
#### Prerequisites
Installing Webpack and TypeScript globally

    npm install -g webpack
    npm install -g typescript

#### Hosting the library view
- Build Dynamo solution and produce `HostedContents.dll` assembly

- Build source scripts

    src/HostedContents> tsc
    src/HostedContents> webpack

- Serve up the new library view

    `node .\index.js`

- Launch `DynamoSandbox.exe`