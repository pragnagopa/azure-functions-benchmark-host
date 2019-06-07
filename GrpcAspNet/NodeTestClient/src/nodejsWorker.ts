console.log("Node process version: " + process.version);
let worker = require("./Worker.js");
console.log("starting with args " + process.argv);
worker.startNodeWorker(process.argv);
