// Vue CLI 3 leaves legacy worker/socket handles alive on current Node versions.
// Wait for its build promise (including output writes) before terminating.
const path = require('path');
const Service = require('@vue/cli-service');
const service = new Service(path.resolve(__dirname, '..'));
service.run('build', {}, []).then(
  () => process.exit(0),
  error => {
    console.error(error);
    process.exit(1);
  }
);
