import adapter from '@sveltejs/adapter-static';
import { vitePreprocess } from '@sveltejs/vite-plugin-svelte';
import { mdsvex } from 'mdsvex';
import { remarkVars } from '@nocturne/cms/remark/vars';

/** @type {import('@sveltejs/kit').Config} */
const config = {
  preprocess: [vitePreprocess(), mdsvex({ extensions: ['.svx'], remarkPlugins: [remarkVars] })],
  extensions: ['.svelte', '.svx'],
  kit: {
    adapter: adapter({
      pages: 'build',
      assets: 'build',
      fallback: '404.html',
    }),
    paths: {
      base: process.env.BASE_PATH ?? '',
    },
    prerender: {
      handleHttpError: ({ path, message }) => {
        if (path === '/setup') return;
        throw new Error(message);
      },
    },
  },
  compilerOptions: {
    experimental: {
      async: true,
    },
  },
};

export default config;
