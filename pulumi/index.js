"use strict";
const k8s = require("@pulumi/kubernetes");

const pulumi = require("@pulumi/pulumi");

let config = new pulumi.Config("k8s-orleans");

const imageTag = config.require("image-tag");  

const clusterVersionCRD = new k8s.apiextensions.v1beta1.CustomResourceDefinition("clusterversions.orleans.dot.net", {
    "metadata": {
      "name": "clusterversions.orleans.dot.net"
    },
    "spec": {
      "group": "orleans.dot.net",
      "version": "v1",
      "scope": "Namespaced",
      "names": {
        "plural": "clusterversions",
        "singular": "clusterversion",
        "kind": "OrleansClusterVersion",
        "shortNames": [
          "ocv"
        ]
      }
    }
});

const siloEntryCRD = new k8s.apiextensions.v1beta1.CustomResourceDefinition("silos.orleans.dot.net", {
    "metadata": {
      "name": "silos.orleans.dot.net"
    },
    "spec": {
      "group": "orleans.dot.net",
      "version": "v1",
      "scope": "Namespaced",
      "names": {
        "plural": "silos",
        "singular": "silo",
        "kind": "OrleansSilo",
        "shortNames": [
          "oso"
        ]
      }
    }
});

const siloLables = { "app": "silo"};
const siloService = new k8s.core.v1.Service("silo", {
    "metadata": {
      "name": "silo",
      "labels": siloLables
    },
    "spec": {
      "ports": [
        {
          "port": 11111,
          "targetPort": 11111,
          "protocol": "TCP",
          "name": "silo-port"
        },
        {
          "port": 30000,
          "targetPort": 30000,
          "protocol": "TCP",
          "name": "gateway-port"
        }
      ],
      "selector": siloLables
    }
});
const siloDeployment = new k8s.apps.v1.Deployment("silo", {
    "metadata": {
      "name": "silo",
      "labels": {
        "app": "silo"
      }
    },
    "spec": {
      "replicas": 2,
      "selector": {
        "matchLabels": siloLables
      },
      "template": {
        "metadata": {
          "labels": siloLables
        },
        "spec": {
          "containers": [
            {
              "name": "silo",
              "image": "silo:" + imageTag,
              "ports": [
                {
                  "containerPort": 1111
                },
                {
                  "containerPort": 30000
                }
              ]
            }
          ]
        }
      }
    }
});

const apiLabels = { "app" : "api"};
const apiService = new k8s.core.v1.Service("api", {
    "metadata": {
      "name": "api",
      "labels": apiLabels
    },
    "spec": {
      "type": "NodePort",
      "ports": [
        {
          "port": 80,
          "targetPort": 80,
          "protocol": "TCP",
          "name": "api-port"
        }
      ],
      "selector": apiLabels
    }
});
const apiDeployment = new k8s.apps.v1.Deployment("api", {
    "etadata": {
      "name": "api",
      "labels": {
        "app": "api"
      }
    },
    "spec": {
      "replicas": 1,
      "selector": {
        "matchLabels": apiLabels
      },
      "template": {
        "metadata": {
          "labels": apiLabels
        },
        "spec": {
          "containers": [
            {
              "name": "api",
              "image": "api:" + imageTag,
              "ports": [
                {
                  "containerPort": 80
                }
              ]
            }
          ]
        }
      }
    }
});

exports.ip = apiDeployment.metadata.apply(m => m.name);
