import 'zone.js';
import 'zone.js/testing';

declare const require: {
  context(path: string, deep?: boolean, filter?: RegExp): {
    <T>(id: string): T;
    keys(): string[];
  };
};

const context = require.context('./', true, /\.spec\.ts$/);
context.keys().forEach(context);