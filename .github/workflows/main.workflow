workflow "Publish to Surge.sh" {
  on = "push"
  resolves = ["Publish to awesome-actions.surge.sh"]
}

action "Filters for GitHub Actions" {
  uses = "actions/bin/filter@e96fd9a"
  args = "branch master"
}

action "Build static website" {
  uses = "docker://jekyll/jekyll"
  needs = ["Filters for GitHub Actions"]
  runs = "jekyll build"
}

action "Publish to awesome-actions.surge.sh" {
  uses = "./.github/actions/surge"
  needs = ["Build static website"]
  runs = "surge _site awesome-actions.surge.sh"
  secrets = ["SURGE_TOKEN"]
  env = {
    SURGE_LOGIN = "ichunlea@me.com"
  }
}
