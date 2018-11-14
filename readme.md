# deploy CRDs
## this must be done before deploying cluster
skaffold run -f skaffold-crds.yaml

# deploy orleans cluster
skaffold run or skaffold dev