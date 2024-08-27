const fs = require('fs');
const path = require('path');

//todo:KO; make an array and then pass in all 'basic' folders like wwwroot, etc
const dir = path.join(__dirname, 'wwwroot', 'lib');

if (!fs.existsSync(dir))
{
    fs.mkdirSync(dir, {recursive: true});
}