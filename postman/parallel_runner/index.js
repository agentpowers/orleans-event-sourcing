const path = require("path")
const async = require("async")
const newman = require("newman")
const fs = require("fs")

let collectionJson = JSON.parse(fs.readFileSync("../ORLEANS_Account.postman_collection.json"));
// when using iterationData, filter out methods that are not in csv(or JSON)
collectionJson.item[0].item = collectionJson.item[0].item.filter(i => (i.name !== "TRANSFER"));

const PARALLEL_RUN_COUNT = 3

const parametersForTestRun = {
    collection: collectionJson, // collection
    environment: path.join(__dirname, "../local.environment.json"), // env
    globals: path.join(__dirname, "../globals.json"), //global
    iterationData: path.join(__dirname, "../postman-runner3.csv"), //iteration
    reporters: "cli"
};

console.log(parametersForTestRun);

parallelCollectionRun = function (done) {
    newman.run(parametersForTestRun, done);
};

let commands = []
for (let index = 0; index < PARALLEL_RUN_COUNT; index++) {
    commands.push(parallelCollectionRun);
}

// Runs the Postman sample collections in parallel.
async.parallel(
    commands,
    (err, results) => {
        err && console.error(err);

        results.forEach(function (result) {
            var failures = result.run.failures;
            console.info(failures.length ? JSON.stringify(failures.failures, null, 2) :
                `${result.collection.name} ran successfully.`);
        });
    });