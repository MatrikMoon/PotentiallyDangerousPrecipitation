import { exec } from 'child_process';
import { app } from 'electron';
import extract from 'extract-zip';
import find from 'find-process';
import { existsSync, rmdir, unlink } from 'fs-extra';
import path from 'path';
import { promisify } from 'util';
import { findSteamAppById } from './find-steam-app';

const RESOURCES_PATH = app.isPackaged
    ? path.join(process.resourcesPath, 'assets')
    : path.join(__dirname, '../../assets');

const getAssetPath = (...paths: string[]): string => {
    return path.join(RESOURCES_PATH, ...paths);
};

const getGamePath = async () => {
    return findSteamAppById(632360);
};

const extractIpaToGameDirectory = async () => {
    await extract(getAssetPath('IPA.zip'), { dir: (await getGamePath())! });
};

const runIpaWithGameAsParameter = async () => {
    const ipaPath = path.join((await getGamePath())!, 'RoRIPA.exe');
    const gameExecutablePath = path.join((await getGamePath())!, 'Risk of Rain 2.exe');

    const execPromise = promisify(exec);
    const { stdout } = await execPromise(`"${ipaPath}" "${gameExecutablePath}"`);
    console.log(stdout);
};

export const killGameIfRunning = async () => {
    const riskOfRainProcesses = await find('name', 'Risk of Rain 2');
    riskOfRainProcesses.forEach((riskOfRainProcess) => {
        process.kill(riskOfRainProcess.pid);
    });
};

export const isGameInstalled = async () => {
    return (await getGamePath()) != null;
};

//TODO: It would be better if we checked if the game was actually patched with IPA, but this will do for now
export const isGameModded = async () => {
    return (await isGameInstalled()) && existsSync(path.join((await getGamePath())!, 'RoRIPA.exe'));
};

export const patchGame = async () => {
    await killGameIfRunning();
    await extractIpaToGameDirectory();
    await runIpaWithGameAsParameter();
};

export const unpatchGame = async () => {
    await killGameIfRunning();

    const gamePath = (await getGamePath())!;
    const ipaExecutablePath = path.join(gamePath, 'RoRIPA.exe');
    const ipaExecutableConfigPath = path.join(gamePath, 'RoRIPA.exe.config');
    const ipaExecutablePdbPath = path.join(gamePath, 'RoRIPA.pdb');
    const ipaFolderPath = path.join(gamePath, 'IPA');
    const pluginsFolderPath = path.join(gamePath, 'Plugins');
    const monoCecilPath = path.join(gamePath, 'Mono.Cecil.dll');

    const gameExecutablePath = path.join((await getGamePath())!, 'Risk of Rain 2.exe');
    const execPromise = promisify(exec);
    const { stdout } = await execPromise(`"${ipaExecutablePath}" "${gameExecutablePath}" --revert --nowait`);
    console.log(stdout);

    await unlink(ipaExecutablePath);
    await unlink(ipaExecutableConfigPath);
    await unlink(ipaExecutablePdbPath);
    await unlink(monoCecilPath);
    await rmdir(ipaFolderPath, { recursive: true });
    await rmdir(pluginsFolderPath, { recursive: true });
};
