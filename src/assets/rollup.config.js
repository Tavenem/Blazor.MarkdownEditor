import { nodeResolve } from '@rollup/plugin-node-resolve';
import commonjs from '@rollup/plugin-commonjs';
import { terser } from 'rollup-plugin-terser';

let plugins = [
    nodeResolve(),
    commonjs(),
];
if (process.env.build === 'Release') {
    plugins.push(terser());
}

export default [{
    input: "./tavenem-mde.js",
    output: {
        format: 'es',
        sourcemap: true,
    },
    plugins: plugins,
}];